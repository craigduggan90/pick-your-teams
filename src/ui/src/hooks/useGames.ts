import { useInfiniteQuery } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { getGames, type GetGamesParams } from '@/api/games'

export function gamesQueryKey(params: GetGamesParams) {
  return ['games', params] as const
}

export function useGames(params: GetGamesParams) {
  const { getAccessTokenSilently } = useAuth0()

  return useInfiniteQuery({
    queryKey: gamesQueryKey(params),
    queryFn: async ({ pageParam }: { pageParam: string | undefined }) => {
      const token = await getAccessTokenSilently()
      return getGames({ ...params, cursor: pageParam }, token)
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) => lastPage.cursor ?? undefined,
  })
}
