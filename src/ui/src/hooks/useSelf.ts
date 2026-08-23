import { useQuery } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { getSelf } from '@/api/users'

export const selfQueryKey = ['self'] as const

const SELF_POLL_INTERVAL_MS = 60_000

export function useSelf() {
  const { isAuthenticated, getAccessTokenSilently } = useAuth0()

  return useQuery({
    queryKey: selfQueryKey,
    queryFn: async () => {
      const token = await getAccessTokenSilently()
      return getSelf(token)
    },
    enabled: isAuthenticated,
    // useSelf is called from ~10 places (Header, TagGate, most pages) - without staleTime,
    // every route navigation remounts a "stale" query and refetches. staleTime here means a
    // navigation within the poll window reuses the cached value instead of firing a new request;
    // refetchInterval keeps it fresh in the background regardless of navigation/focus.
    staleTime: SELF_POLL_INTERVAL_MS,
    refetchInterval: SELF_POLL_INTERVAL_MS,
  })
}
