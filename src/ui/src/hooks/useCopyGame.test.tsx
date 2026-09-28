import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { copyGame, type GameModel } from '@/api/games'
import { useCopyGame } from './useCopyGame'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/games')

function makeWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }
}

describe('useCopyGame', () => {
  it('POSTs the new start time and invalidates the games query', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    const copiedGame = { id: 'game-2' } as GameModel
    vi.mocked(copyGame).mockResolvedValue(copiedGame)

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries')

    const { result } = renderHook(() => useCopyGame('game-1'), {
      wrapper: makeWrapper(queryClient),
    })

    result.current.mutate({ StartTime: '2026-08-17T20:00:00.000Z' })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(copyGame).toHaveBeenCalledWith(
      'game-1',
      { StartTime: '2026-08-17T20:00:00.000Z' },
      'token123',
    )
    expect(result.current.data).toEqual(copiedGame)
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['games'] })
  })
})
