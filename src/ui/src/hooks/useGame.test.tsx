import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { getGameById } from '@/api/games'
import { useGame } from './useGame'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/games')

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

describe('useGame', () => {
  it('does not fetch when id is undefined', () => {
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently: vi.fn() } as any)

    renderHook(() => useGame(undefined), { wrapper })

    expect(getGameById).not.toHaveBeenCalled()
  })

  it('fetches the game detail for the given id', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(getGameById).mockResolvedValue({ id: 'game-1' } as any)

    const { result } = renderHook(() => useGame('game-1'), { wrapper })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(getGameById).toHaveBeenCalledWith('game-1', 'token123')
    expect(result.current.data?.id).toBe('game-1')
  })
})
