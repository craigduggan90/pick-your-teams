import { useMutation } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { createInvitations } from '@/api/invitations'
import type { ApiError } from '@/api/client'

export function useCreateInvitations(gameId: string) {
  const { getAccessTokenSilently } = useAuth0()

  return useMutation<void, ApiError, string[]>({
    mutationFn: async (userTags) => {
      const token = await getAccessTokenSilently()
      await createInvitations({ GameId: gameId, UserTags: userTags }, token)
    },
  })
}
