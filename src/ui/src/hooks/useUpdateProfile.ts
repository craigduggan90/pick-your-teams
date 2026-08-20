import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { updateUser } from '@/api/users'
import type { ApiError } from '@/api/client'
import { selfQueryKey } from './useSelf'

export interface UpdateProfileInput {
  DisplayName?: string
  Email?: string
}

export function useUpdateProfile(userId: string | undefined) {
  const { getAccessTokenSilently } = useAuth0()
  const queryClient = useQueryClient()

  return useMutation<void, ApiError, UpdateProfileInput>({
    mutationFn: async (body) => {
      if (!userId) {
        throw new Error('useUpdateProfile called before the current user is known')
      }
      const token = await getAccessTokenSilently()
      await updateUser(userId, body, token)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: selfQueryKey })
    },
  })
}
