import { cn } from '@/lib/utils'
import type { InvitationStatus } from '@/api/invitations'

// "Open" reads as an internal/API name, not something an organiser would recognise on a status
// list — "Pending" matches the still-awaiting-a-response meaning without exposing the enum name.
const STATUS_LABELS: Record<InvitationStatus, string> = {
  Open: 'Pending',
  Accepted: 'Accepted',
  Declined: 'Declined',
  Failed: 'Failed',
}

const STATUS_STYLES: Record<InvitationStatus, string> = {
  Open: 'bg-info/10 text-info',
  Accepted: 'bg-success/10 text-success',
  // Declined is a normal outcome (the invitee just said no), not an error - kept neutral rather
  // than reusing the Error token, which is reserved for Failed (a real delivery/validation
  // problem, see Invitation.DispatchError on the backend).
  Declined: 'bg-light-grey/10 text-light-grey',
  Failed: 'bg-error/10 text-error',
}

export function InvitationStatusBadge({ status, className }: { status: InvitationStatus; className?: string }) {
  return (
    <span className={cn('rounded-full px-2 py-0.5 text-xs font-medium', STATUS_STYLES[status], className)}>
      {STATUS_LABELS[status]}
    </span>
  )
}
