import { cn } from '@/lib/utils'
import type { GameStatus } from '@/api/games'

// Diagram labels this "Complete"; the real GameStatusEnum member is "Finished" (there's no
// separate "Complete" status) — display copy only.
const STATUS_LABELS: Record<GameStatus, string> = {
  Scheduled: 'Scheduled',
  Finished: 'Finished',
}

const STATUS_STYLES: Record<GameStatus, string> = {
  Scheduled: 'bg-info/10 text-info',
  Finished: 'bg-warning/10 text-warning',
}

export function GameStatusBadge({ status, className }: { status: GameStatus; className?: string }) {
  return (
    <span
      className={cn(
        'rounded-full px-2 py-0.5 text-xs font-medium',
        STATUS_STYLES[status],
        className,
      )}
    >
      {STATUS_LABELS[status]}
    </span>
  )
}
