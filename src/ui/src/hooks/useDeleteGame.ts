import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { deleteGame } from '@/api/games'
import type { ApiError } from '@/api/client'

export function useDeleteGame(id: string) {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<void, ApiError, void>({
    mutationFn: async () => {
      const token = await getAccessTokenSilently()
      await deleteGame(id, token)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['games'] })
    },
  })
}
