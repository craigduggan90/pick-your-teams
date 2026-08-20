import { Sheet } from '@/components/Sheet'
import { Button } from '@/components/Button'
import type { GameTeamPlayerModel } from '@/api/games'

export interface RemovePlayerModalProps {
  player: GameTeamPlayerModel | null
  onOpenChange: (open: boolean) => void
  onConfirm: (player: GameTeamPlayerModel) => void
  isPending: boolean
}

// Reuses the shared Sheet shell — the "Remove @Tag?" pattern from 06-a-view-teams.png / claude.md.
// Only rendered for a User-linked player (has a Tag); a Dummy player is removed with no
// confirmation, so this component is never mounted for one.
export function RemovePlayerModal({
  player,
  onOpenChange,
  onConfirm,
  isPending,
}: RemovePlayerModalProps) {
  return (
    <Sheet
      open={player !== null}
      onOpenChange={onOpenChange}
      title={player ? `Remove @${player.tag}?` : 'Remove player?'}
      description={`${player?.displayName ?? 'This player'} will need a new invite to re-join the game. Are you sure?`}
      footer={
        <>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={isPending}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            disabled={isPending}
            onClick={() => player && onConfirm(player)}
          >
            {isPending ? 'Removing…' : 'Remove'}
          </Button>
        </>
      }
    />
  )
}
