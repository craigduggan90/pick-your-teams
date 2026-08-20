import { useMutation } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { deleteUser } from '@/api/users'
import type { ApiError } from '@/api/client'

export function useDeleteAccount(userId: string | undefined) {
  const { getAccessTokenSilently } = useAuth0()

  return useMutation<void, ApiError, void>({
    mutationFn: async () => {
      if (!userId) {
        throw new Error('useDeleteAccount called before the current user is known')
      }
      const token = await getAccessTokenSilently()
      await deleteUser(userId, token)
    },
  })
}
