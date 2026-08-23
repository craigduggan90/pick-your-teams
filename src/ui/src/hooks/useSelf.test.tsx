import type { ReactNode } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { getSelf, type UserDetailModel } from '@/api/users'
import { useSelf } from './useSelf'

vi.mock('@auth0/auth0-react')
vi.mock('@/api/users')

const selfFixture: UserDetailModel = {
  id: '1',
  tag: '1',
  displayName: 'Dev User',
  rating: 0,
  email: 'dev@example.com',
  mobile: null,
  created: '2026-01-01T00:00:00Z',
  modified: '2026-01-01T00:00:00Z',
  pendingInvitations: 0,
}

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

function mockAuthenticated() {
  const getAccessTokenSilently = vi.fn().mockResolvedValue('token123')
  vi.mocked(useAuth0).mockReturnValue({
    isAuthenticated: true,
    getAccessTokenSilently,
  } as any)
  return getAccessTokenSilently
}

describe('useSelf', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('does not fetch when the user is not authenticated', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: false,
      getAccessTokenSilently: vi.fn(),
    } as any)

    renderHook(() => useSelf(), { wrapper })

    expect(getSelf).not.toHaveBeenCalled()
  })

  it('fetches the current user once authenticated', async () => {
    const getAccessTokenSilently = mockAuthenticated()
    vi.mocked(getSelf).mockResolvedValue(selfFixture)

    const { result } = renderHook(() => useSelf(), { wrapper })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(getAccessTokenSilently).toHaveBeenCalled()
    expect(getSelf).toHaveBeenCalledWith('token123')
    expect(result.current.data?.id).toBe('1')
  })

  it('does not refetch when a second component mounts within the poll window', async () => {
    mockAuthenticated()
    vi.mocked(getSelf).mockResolvedValue(selfFixture)

    // A shared client, unlike the module-level `wrapper` helper - staleTime only has anything to
    // say about a remount if the cache (and therefore the client) actually persists across it,
    // the same way it would for two components (e.g. Header + a page) mounted under the real app.
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const sharedWrapper = ({ children }: { children: ReactNode }) => (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    )

    const first = renderHook(() => useSelf(), { wrapper: sharedWrapper })
    await waitFor(() => expect(first.result.current.isSuccess).toBe(true))
    first.unmount()

    const second = renderHook(() => useSelf(), { wrapper: sharedWrapper })
    await waitFor(() => expect(second.result.current.isSuccess).toBe(true))

    expect(getSelf).toHaveBeenCalledTimes(1)
  })

  it('refetches automatically once the poll interval elapses', async () => {
    // Fake timers from the start, not switched on partway through - TanStack Query schedules
    // the next refetch off the real clock at the moment the first fetch resolves, so enabling
    // fake time only after that point leaves its internal "next fetch at" reference on a
    // different clock than vi.advanceTimersByTimeAsync is advancing.
    vi.useFakeTimers()
    try {
      mockAuthenticated()
      vi.mocked(getSelf).mockResolvedValue(selfFixture)

      const { result } = renderHook(() => useSelf(), { wrapper })
      await vi.waitFor(() => expect(result.current.isSuccess).toBe(true))
      expect(getSelf).toHaveBeenCalledTimes(1)

      await vi.advanceTimersByTimeAsync(60_000)

      expect(getSelf).toHaveBeenCalledTimes(2)
    } finally {
      vi.useRealTimers()
    }
  })
})
