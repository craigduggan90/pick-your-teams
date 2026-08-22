import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { declineInvitation } from '@/api/invitations'
import type { ApiError } from '@/api/client'
import { selfQueryKey } from './useSelf'

export function useDeclineInvitation() {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<void, ApiError, string>({
    mutationFn: async (invitationId) => {
      const token = await getAccessTokenSilently()
      await declineInvitation(invitationId, token)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invitations'] })
      queryClient.invalidateQueries({ queryKey: selfQueryKey })
    },
  })
}
