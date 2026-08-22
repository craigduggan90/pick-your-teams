import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router'
import { Button } from '@/components/Button'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'
import { GameInviteListItem } from '@/components/GameInviteListItem'
import { useGame } from '@/hooks/useGame'
import { useSelf } from '@/hooks/useSelf'
import { useInvitations } from '@/hooks/useInvitations'
import { usePageTitle } from '@/hooks/usePageTitle'
import { usePageFooterActions } from '@/hooks/usePageActions'

// Route /games/:id/invites, reached from Game View's "View Invites" button (organiser-only).
// GetInvitations' gameId filter is already ownership-guarded server-side to the game's organiser,
// but a non-organiser could still land here directly by URL - same as InvitePlayersPage, bounce
// them back to Teams rather than leaving the page stuck on Loading forever (the invitations query
// never enables for a non-organiser, so without this redirect there'd be nothing to show them).
export function GameInvitesPage() {
  usePageTitle('Invites')
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const gameQuery = useGame(id)
  const selfQuery = useSelf()

  const game = gameQuery.data
  const isOrganiser = Boolean(
    game?.organiser && selfQuery.data && game.organiser.id === selfQuery.data.id,
  )

  useEffect(() => {
    if (game && !isOrganiser) {
      navigate(`/games/${id}/teams`, { replace: true })
    }
  }, [game, isOrganiser, id, navigate])

  const invitationsQuery = useInvitations(
    { gameId: id },
    { enabled: Boolean(id) && isOrganiser },
  )

  usePageFooterActions(
    <div className="flex w-full p-4">
      <Button variant="outline" onClick={() => navigate(`/games/${id}`)}>
        Back
      </Button>
    </div>,
  )

  if (gameQuery.isPending || selfQuery.isPending) {
    return <Loading />
  }

  if (gameQuery.isError || !game) {
    return <ErrorMessage>Something went wrong loading this game.</ErrorMessage>
  }

  if (invitationsQuery.isPending) {
    return <Loading />
  }

  if (invitationsQuery.isError) {
    return <ErrorMessage>Something went wrong loading these invites.</ErrorMessage>
  }

  const invitations = invitationsQuery.data?.pages.flatMap((page) => page.data) ?? []

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col p-4">
      {invitations.length === 0 && (
        <p className="p-4 text-center text-sm text-light-grey">No Invites Sent Yet!</p>
      )}
      {invitations.length > 0 && (
        <div className="flex flex-col gap-3">
          {invitations.map((invitation) => (
            <GameInviteListItem key={invitation.id} invitation={invitation} />
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
