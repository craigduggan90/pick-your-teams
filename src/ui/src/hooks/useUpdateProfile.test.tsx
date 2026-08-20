import type { ReactNode } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { updateUser } from '@/api/users'
import { useUpdateProfile } from './useUpdateProfile'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/users')

function makeWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }
}

describe('useUpdateProfile', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('PATCHes the display name/email for the given user and invalidates the self query', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(updateUser).mockResolvedValue(undefined)

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries')

    const { result } = renderHook(() => useUpdateProfile('user-1'), {
      wrapper: makeWrapper(queryClient),
    })

    result.current.mutate({ DisplayName: 'Mike Rotch', Email: 'mike@example.com' })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(updateUser).toHaveBeenCalledWith(
      'user-1',
      { DisplayName: 'Mike Rotch', Email: 'mike@example.com' },
      'token123',
    )
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['self'] })
  })

  it('rejects when called before the current user id is known', async () => {
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently: vi.fn() } as any)

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { result } = renderHook(() => useUpdateProfile(undefined), {
      wrapper: makeWrapper(queryClient),
    })

    result.current.mutate({ DisplayName: 'Mike Rotch' })

    await waitFor(() => expect(result.current.isError).toBe(true))

    expect(updateUser).not.toHaveBeenCalled()
  })
})
