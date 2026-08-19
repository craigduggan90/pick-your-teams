import { useQuery } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { getSelf } from '@/api/users'

export const selfQueryKey = ['self'] as const

export function useSelf() {
  const { isAuthenticated, getAccessTokenSilently } = useAuth0()

  return useQuery({
    queryKey: selfQueryKey,
    queryFn: async () => {
      const token = await getAccessTokenSilently()
      return getSelf(token)
    },
    enabled: isAuthenticated,
  })
}
