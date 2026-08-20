import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { updateGame, type UpdateGameRequestModel } from '@/api/games'
import type { ApiError } from '@/api/client'
import { gameQueryKey } from './useGame'

export function useUpdateGame(id: string) {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<void, ApiError, UpdateGameRequestModel>({
    mutationFn: async (body) => {
      const token = await getAccessTokenSilently()
      await updateGame(id, body, token)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: gameQueryKey(id) })
      queryClient.invalidateQueries({ queryKey: ['games'] })
    },
  })
}
