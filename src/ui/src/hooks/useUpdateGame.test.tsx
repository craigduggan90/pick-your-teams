import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { updateGame } from '@/api/games'
import { useUpdateGame } from './useUpdateGame'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/games')

function makeWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }
}

describe('useUpdateGame', () => {
  it('PATCHes the game and invalidates the game and games queries', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(updateGame).mockResolvedValue(undefined)

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries')

    const { result } = renderHook(() => useUpdateGame('game-1'), {
      wrapper: makeWrapper(queryClient),
    })

    result.current.mutate({ Location: 'The Pitch' })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(updateGame).toHaveBeenCalledWith('game-1', { Location: 'The Pitch' }, 'token123')
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['game', 'game-1'] })
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['games'] })
  })
})
