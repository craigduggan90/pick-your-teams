import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { getInvitations } from '@/api/invitations'
import { useInvitations } from './useInvitations'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/invitations')

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

describe('useInvitations', () => {
  it('fetches the first page with no cursor', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(getInvitations).mockResolvedValue({
      data: [{ id: 'inv-1' }] as any,
      cursor: null,
      count: 1,
    })

    const { result } = renderHook(() => useInvitations({ userId: 'user-1', status: 'Open' }), {
      wrapper,
    })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(getInvitations).toHaveBeenCalledWith(
      { userId: 'user-1', status: 'Open', cursor: undefined },
      'token123',
    )
    expect(result.current.data?.pages[0].data).toEqual([{ id: 'inv-1' }])
  })

  it('fetches the next page using the previous page cursor', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(getInvitations).mockResolvedValue({ data: [], cursor: 'next-cursor', count: 0 })

    const { result } = renderHook(() => useInvitations({}), { wrapper })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.hasNextPage).toBe(true)

    await result.current.fetchNextPage()

    expect(getInvitations).toHaveBeenLastCalledWith({ cursor: 'next-cursor' }, 'token123')
  })
})
