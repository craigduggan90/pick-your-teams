import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { recordResult } from '@/api/games'
import { useRecordResult } from './useRecordResult'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/games')

function makeWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }
}

describe('useRecordResult', () => {
  it('POSTs the winner and invalidates the game and games queries', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({ getAccessTokenSilently } as any)
    vi.mocked(recordResult).mockResolvedValue(undefined)

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries')

    const { result } = renderHook(() => useRecordResult('game-1'), {
      wrapper: makeWrapper(queryClient),
    })

    result.current.mutate('None')

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(recordResult).toHaveBeenCalledWith('game-1', 'None', 'token123')
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['game', 'game-1'] })
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['games'] })
  })
})
