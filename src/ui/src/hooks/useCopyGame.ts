import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { copyGame, type CopyGameRequestModel, type GameModel } from '@/api/games'
import type { ApiError } from '@/api/client'

export function useCopyGame(id: string) {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<GameModel, ApiError, CopyGameRequestModel>({
    mutationFn: async (body) => {
      const token = await getAccessTokenSilently()
      return copyGame(id, body, token)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['games'] })
    },
  })
}
