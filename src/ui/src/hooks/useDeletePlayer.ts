import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { deletePlayer } from '@/api/players'
import type { ApiError } from '@/api/client'
import { gameTeamsQueryKey } from './useGameTeams'

export function useDeletePlayer(gameId: string) {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<void, ApiError, string>({
    mutationFn: async (playerId) => {
      const token = await getAccessTokenSilently()
      await deletePlayer(playerId, token)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: gameTeamsQueryKey(gameId) })
    },
  })
}
