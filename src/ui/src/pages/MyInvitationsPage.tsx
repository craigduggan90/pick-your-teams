import { useEffect } from 'react'
import { useNavigate } from 'react-router'
import { Button } from '@/components/Button'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'
import { InvitationListItem } from '@/components/InvitationListItem'
import { toast } from '@/components/Toast'
import { useSelf } from '@/hooks/useSelf'
import { useInvitations } from '@/hooks/useInvitations'
import { useAcceptInvitation } from '@/hooks/useAcceptInvitation'
import { useDeclineInvitation } from '@/hooks/useDeclineInvitation'
import { usePageTitle } from '@/hooks/usePageTitle'
import { usePageFooterActions } from '@/hooks/usePageActions'
import { ApiError } from '@/api/client'

// Both Accept and Decline can 422 two ways — the invitation already resolved to the opposite
// outcome, or (accept only) the game is already at capacity. Neither carries a field-level
// `errors` dict (they come from RequestHandlerException, not FluentValidation), so the message
// is always `detail` directly rather than something to flatten out of `errors`.
function apiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof ApiError)) return fallback
  return error.problem.detail ?? error.message
}

// Route /invitations, from the persistent Header's icon (see Header.tsx). Only ever shows Open
// invitations for the current user — resolved ones don't reappear on subsequent GET /invitations
// loads server-side, so no client-side filtering is needed beyond the status=Open request itself.
export function MyInvitationsPage() {
  usePageTitle('My Invitations')
  const navigate = useNavigate()
  const selfQuery = useSelf()
  const invitationsQuery = useInvitations(
    { userId: selfQuery.data?.id, status: 'Open' },
    { enabled: Boolean(selfQuery.data) },
  )
  const acceptMutation = useAcceptInvitation()
  const declineMutation = useDeclineInvitation()

  useEffect(() => {
    if (acceptMutation.isError) {
      toast.error(apiErrorMessage(acceptMutation.error, 'Something went wrong accepting this invitation.'))
    }
  }, [acceptMutation.isError, acceptMutation.error])

  useEffect(() => {
    if (declineMutation.isError) {
      toast.error(apiErrorMessage(declineMutation.error, 'Something went wrong declining this invitation.'))
    }
  }, [declineMutation.isError, declineMutation.error])

  usePageFooterActions(
    <div className="flex w-full p-4">
      <Button variant="outline" onClick={() => navigate('/')}>
        Back
      </Button>
    </div>,
  )

  const invitations = invitationsQuery.data?.pages.flatMap((page) => page.data) ?? []
  const isLoading = selfQuery.isPending || invitationsQuery.isPending

  if (isLoading) {
    return <Loading />
  }

  if (selfQuery.isError || invitationsQuery.isError) {
    return <ErrorMessage>Something went wrong loading your invitations.</ErrorMessage>
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col p-4">
      {invitations.length === 0 && (
        <p className="p-4 text-center text-sm text-light-grey">No Invitations Found!</p>
      )}
      {invitations.length > 0 && (
        <div className="flex flex-col gap-3">
          {invitations.map((invitation) => (
            <InvitationListItem
              key={invitation.id}
              invitation={invitation}
              onAccept={() => acceptMutation.mutate(invitation.id)}
              onDecline={() => declineMutation.mutate(invitation.id)}
              isAccepting={acceptMutation.isPending && acceptMutation.variables === invitation.id}
              isDeclining={declineMutation.isPending && declineMutation.variables === invitation.id}
            />
          ))}
          {invitationsQuery.hasNextPage && (
            <Button
              variant="outline"
              className="w-full"
              onClick={() => invitationsQuery.fetchNextPage()}
              disabled={invitationsQuery.isFetchingNextPage}
            >
              {invitationsQuery.isFetchingNextPage ? 'Loading…' : 'Load More…'}
            </Button>
          )}
        </div>
      )}
    </div>
  )
}
