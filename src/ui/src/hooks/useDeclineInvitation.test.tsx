import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { declineInvitation } from '@/api/invitations'
import { useDeclineInvitation } from './useDeclineInvitation'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/invitations')

function makeWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }
}

describe('useDeclineInvitation', () => {
  it('declines the invitation and invalidates invitations + self', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(declineInvitation).mockResolvedValue(undefined)

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries')

    const { result } = renderHook(() => useDeclineInvitation(), {
      wrapper: makeWrapper(queryClient),
    })

    result.current.mutate('inv-1')

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(declineInvitation).toHaveBeenCalledWith('inv-1', 'token123')
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['invitations'] })
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['self'] })
  })
})
