import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { deleteGame } from '@/api/games'
import { useDeleteGame } from './useDeleteGame'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/games')

function makeWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }
}

describe('useDeleteGame', () => {
  it('DELETEs the game and invalidates the games query', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(deleteGame).mockResolvedValue(undefined)

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries')

    const { result } = renderHook(() => useDeleteGame('game-1'), {
      wrapper: makeWrapper(queryClient),
    })

    result.current.mutate()

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(deleteGame).toHaveBeenCalledWith('game-1', 'token123')
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['games'] })
  })
})
