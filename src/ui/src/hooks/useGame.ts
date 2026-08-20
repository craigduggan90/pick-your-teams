import { useQuery } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { getGameById } from '@/api/games'

export function gameQueryKey(id: string | undefined) {
  return ['game', id] as const
}

export function useGame(id: string | undefined) {
  const { getAccessTokenSilently } = useAuth0()

  return useQuery({
    queryKey: gameQueryKey(id),
    queryFn: async () => {
      const token = await getAccessTokenSilently()
      return getGameById(id!, token)
    },
    enabled: Boolean(id),
  })
}
