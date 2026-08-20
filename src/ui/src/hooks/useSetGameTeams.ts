import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { setGameTeams, type SetTeamsRequestModel } from '@/api/games'
import type { ApiError } from '@/api/client'
import { gameTeamsQueryKey } from './useGameTeams'

export function useSetGameTeams(id: string) {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<void, ApiError, SetTeamsRequestModel>({
    mutationFn: async (body) => {
      const token = await getAccessTokenSilently()
      await setGameTeams(id, body, token)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: gameTeamsQueryKey(id) })
    },
  })
}
