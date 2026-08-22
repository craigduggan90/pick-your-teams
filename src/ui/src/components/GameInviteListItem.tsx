import { InvitationStatusBadge } from './InvitationStatusBadge'
import { formatGameDateTime } from '@/lib/format'
import type { InvitationModel } from '@/api/invitations'

// The organiser-facing counterpart to InvitationListItem: no Accept/Decline (this is the
// organiser's own game, not their invitation to act on), just who was invited, when, and the
// outcome.
export function GameInviteListItem({ invitation }: { invitation: InvitationModel }) {
  return (
    <div className="flex items-center justify-between gap-3 rounded-lg border border-border p-3 text-sm">
      <div className="flex min-w-0 flex-col gap-1">
        <span className="truncate font-medium text-dark-grey">
          {invitation.invitee ? `${invitation.invitee.displayName} (@${invitation.invitee.tag})` : 'Unknown user'}
        </span>
        <span className="truncate text-light-grey">Sent {formatGameDateTime(invitation.created)}</span>
      </div>
      <InvitationStatusBadge status={invitation.status} className="shrink-0" />
    </div>
  )
}
