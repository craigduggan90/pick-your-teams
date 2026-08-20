import { XIcon } from 'lucide-react'
import { Button } from '@/components/Button'
import { GameStatusBadge } from '@/components/GameStatusBadge'
import { formatGameDateTime, formatGameWinner } from '@/lib/format'
import type { GameDetailModel } from '@/api/games'

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between gap-2 text-sm">
      <span className="text-light-grey">{label}</span>
      <span className="font-medium text-dark-grey">{value}</span>
    </div>
  )
}

export interface GameDetailsSheetProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  game: GameDetailModel
  /** Only the organiser gets a way through to the editable game screen — everyone else has no
   * reason to ever land there, so there's no read-only "View Game" variant of this link. */
  showManageLink: boolean
  onManage: () => void
}

// Pops up from the bottom, over the footer, rather than an inline accordion pushing page content
// down — triggered from the footer's "Game Details" button (where "Invite" used to sit).
export function GameDetailsSheet({
  open,
  onOpenChange,
  game,
  showManageLink,
  onManage,
}: GameDetailsSheetProps) {
  if (!open) {
    return null
  }

  return (
    <>
      <div
        className="fixed inset-0 z-40 bg-black/30"
        onClick={() => onOpenChange(false)}
        aria-hidden
      />
      <div
        role="dialog"
        aria-label="Game Details"
        className="animate-in slide-in-from-bottom fixed inset-x-0 bottom-0 z-50 mx-auto flex w-full max-w-md flex-col gap-3 rounded-t-xl border-t border-border bg-background p-4 shadow-lg"
      >
        <div className="flex items-center justify-between">
          <h2 className="font-semibold text-dark-grey">Game Details</h2>
          <button
            type="button"
            aria-label="Close"
            onClick={() => onOpenChange(false)}
            className="cursor-pointer text-light-grey"
          >
            <XIcon className="size-5" />
          </button>
        </div>

        <DetailRow label="Location" value={game.location ?? 'Location TBC'} />
        <DetailRow label="Start Time" value={formatGameDateTime(game.startTime)} />
        <DetailRow label="Duration" value={`${game.duration} minutes`} />
        <DetailRow label="Players Per Team" value={String(game.teamSize)} />
        <div className="flex items-center justify-between text-sm">
          <span className="text-light-grey">Status</span>
          <GameStatusBadge status={game.status} />
        </div>
        {game.status === 'Finished' && game.winner && (
          <DetailRow label="Winner" value={formatGameWinner(game.winner)} />
        )}

        {showManageLink && (
          <Button variant="outline" onClick={onManage}>
            Manage Game
          </Button>
        )}
      </div>
    </>
  )
}
