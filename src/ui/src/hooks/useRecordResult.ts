import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { recordResult, type GameWinner } from '@/api/games'
import type { ApiError } from '@/api/client'
import { gameQueryKey } from './useGame'

export function useRecordResult(id: string) {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<void, ApiError, GameWinner>({
    mutationFn: async (winner) => {
      const token = await getAccessTokenSilently()
      await recordResult(id, winner, token)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: gameQueryKey(id) })
      queryClient.invalidateQueries({ queryKey: ['games'] })
    },
  })
}
