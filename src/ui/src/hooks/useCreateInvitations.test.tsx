import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { createInvitations } from '@/api/invitations'
import { useCreateInvitations } from './useCreateInvitations'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/invitations')

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

describe('useCreateInvitations', () => {
  it('POSTs the game id and the given tags', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(createInvitations).mockResolvedValue(undefined)

    const { result } = renderHook(() => useCreateInvitations('game-1'), { wrapper })

    result.current.mutate(['bob', 'alice'])

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(createInvitations).toHaveBeenCalledWith(
      { GameId: 'game-1', UserTags: ['bob', 'alice'] },
      'token123',
    )
  })
})
