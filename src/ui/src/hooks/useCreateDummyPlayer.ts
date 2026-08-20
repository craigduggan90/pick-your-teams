import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { createDummyPlayer, type PlayerModel } from '@/api/players'
import type { ApiError } from '@/api/client'
import { gameTeamsQueryKey } from './useGameTeams'

export interface CreateDummyPlayerVariables {
  displayName: string
  estimatedRating: number
}

export function useCreateDummyPlayer(gameId: string) {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<PlayerModel, ApiError, CreateDummyPlayerVariables>({
    mutationFn: async ({ displayName, estimatedRating }) => {
      const token = await getAccessTokenSilently()
      return createDummyPlayer(
        { GameId: gameId, DisplayName: displayName, EstimatedRating: estimatedRating },
        token,
      )
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: gameTeamsQueryKey(gameId) })
    },
  })
}
