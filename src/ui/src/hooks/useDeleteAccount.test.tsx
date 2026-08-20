import type { ReactNode } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { deleteUser } from '@/api/users'
import { useDeleteAccount } from './useDeleteAccount'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/users')

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

describe('useDeleteAccount', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('DELETEs the given user', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(deleteUser).mockResolvedValue(undefined)

    const { result } = renderHook(() => useDeleteAccount('user-1'), { wrapper })

    result.current.mutate()

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(deleteUser).toHaveBeenCalledWith('user-1', 'token123')
  })

  it('rejects when called before the current user id is known', async () => {
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently: vi.fn() } as any)

    const { result } = renderHook(() => useDeleteAccount(undefined), { wrapper })

    result.current.mutate()

    await waitFor(() => expect(result.current.isError).toBe(true))

    expect(deleteUser).not.toHaveBeenCalled()
  })
})
