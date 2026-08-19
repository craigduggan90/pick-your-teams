import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { updateUser } from '@/api/users'
import type { ApiError } from '@/api/client'
import { selfQueryKey } from './useSelf'

export function useUpdateTag(userId: string | undefined) {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<void, ApiError, string>({
    mutationFn: async (tag: string) => {
      if (!userId) {
        throw new Error('useUpdateTag called before the current user is known')
      }
      const token = await getAccessTokenSilently()
      await updateUser(userId, { Tag: tag }, token)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: selfQueryKey })
    },
  })
}
