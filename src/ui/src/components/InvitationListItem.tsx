import { CheckIcon, XIcon } from 'lucide-react'
import { formatGameDateTime } from '@/lib/format'
import { cn } from '@/lib/utils'
import type { InvitationModel } from '@/api/invitations'

export interface InvitationListItemProps {
  invitation: InvitationModel
  onAccept: () => void
  onDecline: () => void
  isAccepting: boolean
  isDeclining: boolean
}

// Matches 07-my-invitations.png's row: date/time, "Location | Organiser", accept (check) and
// decline (X) icon buttons. The diagram's "working" dimmed transitional state and the row
// disappearing on success are deliberately out of scope for v1 (see claude.md) — a simple
// disabled state on both buttons during either request is enough.
export function InvitationListItem({
  invitation,
  onAccept,
  onDecline,
  isAccepting,
  isDeclining,
}: InvitationListItemProps) {
  const pending = isAccepting || isDeclining

  return (
    <div className="flex items-center justify-between gap-3 rounded-lg border border-border p-3 text-sm">
      <div className="flex min-w-0 flex-col gap-1">
        <span className="font-medium text-dark-grey">{formatGameDateTime(invitation.game.startTime)}</span>
        <span className="truncate text-light-grey">
          {invitation.game.location ?? 'Location TBC'}
          {invitation.organiser && ` | Organised by @${invitation.organiser.tag}`}
        </span>
      </div>
      <div className="flex shrink-0 gap-2">
        <button
          type="button"
          aria-label="Accept"
          onClick={onAccept}
          disabled={pending}
          className={cn(
            'flex size-9 cursor-pointer items-center justify-center rounded-full bg-success/10 text-success transition-opacity',
            pending && 'cursor-not-allowed opacity-50',
          )}
        >
          <CheckIcon className="size-5" />
        </button>
        <button
          type="button"
          aria-label="Decline"
          onClick={onDecline}
          disabled={pending}
          className={cn(
            'flex size-9 cursor-pointer items-center justify-center rounded-full bg-error/10 text-error transition-opacity',
            pending && 'cursor-not-allowed opacity-50',
          )}
        >
          <XIcon className="size-5" />
        </button>
      </div>
    </div>
  )
}
