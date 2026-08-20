import { useQuery } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { getGameTeams } from '@/api/games'

export function gameTeamsQueryKey(id: string | undefined) {
  return ['gameTeams', id] as const
}

export function useGameTeams(id: string | undefined) {
  const { getAccessTokenSilently } = useAuth0()

  return useQuery({
    queryKey: gameTeamsQueryKey(id),
    queryFn: async () => {
      const token = await getAccessTokenSilently()
      return getGameTeams(id!, token)
    },
    enabled: Boolean(id),
  })
}
