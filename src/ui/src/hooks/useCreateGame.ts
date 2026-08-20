import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { createGame, type CreateGameRequestModel, type GameModel } from '@/api/games'
import type { ApiError } from '@/api/client'

export function useCreateGame() {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<GameModel, ApiError, CreateGameRequestModel>({
    mutationFn: async (body) => {
      const token = await getAccessTokenSilently()
      return createGame(body, token)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['games'] })
    },
  })
}
