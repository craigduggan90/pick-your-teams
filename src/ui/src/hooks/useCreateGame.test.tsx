import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { createGame } from '@/api/games'
import { useCreateGame } from './useCreateGame'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/games')

function makeWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }
}

describe('useCreateGame', () => {
  it('POSTs the game and invalidates the games query', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(createGame).mockResolvedValue({ id: 'game-1' } as any)

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries')

    const { result } = renderHook(() => useCreateGame(), { wrapper: makeWrapper(queryClient) })

    const body = {
      StartTime: '2026-08-10T20:00:00.000Z',
      Duration: 60,
      TeamSize: 5,
      OrganiserId: 'user-1',
    }
    result.current.mutate(body)

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(createGame).toHaveBeenCalledWith(body, 'token123')
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['games'] })
    expect(result.current.data).toEqual({ id: 'game-1' })
  })
})
