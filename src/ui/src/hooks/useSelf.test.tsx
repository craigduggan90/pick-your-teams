import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { getSelf } from '@/api/users'
import { useSelf } from './useSelf'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/users')

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

describe('useSelf', () => {
  it('does not fetch when the user is not authenticated', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: false,
      getAccessTokenSilently: vi.fn(),
    } as any)

    renderHook(() => useSelf(), { wrapper })

    expect(getSelf).not.toHaveBeenCalled()
  })

  it('fetches the current user once authenticated', async () => {
    const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: true,
      getAccessTokenSilently,
    } as any)
    vi.mocked(getSelf).mockResolvedValue({
      Id: '1',
      Tag: '1',
      DisplayName: 'Dev User',
      Rating: 0,
      Email: 'dev@example.com',
      Mobile: null,
      Created: '2026-01-01T00:00:00Z',
      Modified: '2026-01-01T00:00:00Z',
    })

    const { result } = renderHook(() => useSelf(), { wrapper })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(getAccessTokenSilently).toHaveBeenCalled()
    expect(getSelf).toHaveBeenCalledWith('token123')
    expect(result.current.data?.Id).toBe('1')
  })
})
