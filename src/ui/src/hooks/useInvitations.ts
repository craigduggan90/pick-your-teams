import { useInfiniteQuery } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { getInvitations, type GetInvitationsParams } from '@/api/invitations'

export function invitationsQueryKey(params: GetInvitationsParams) {
  return ['invitations', params] as const
}

export function useInvitations(params: GetInvitationsParams, options?: { enabled?: boolean }) {
  const { getAccessTokenSilently } = useAuth0()

  return useInfiniteQuery({
    queryKey: invitationsQueryKey(params),
    queryFn: async ({ pageParam }: { pageParam: string | undefined }) => {
      const token = await getAccessTokenSilently()
      return getInvitations({ ...params, cursor: pageParam }, token)
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) => lastPage.cursor ?? undefined,
    enabled: options?.enabled ?? true,
  })
}
