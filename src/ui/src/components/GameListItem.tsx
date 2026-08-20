import { Link } from 'react-router'
import { GameStatusBadge } from './GameStatusBadge'
import { formatGameDateTime } from '@/lib/format'
import type { GameModel } from '@/api/games'

export function GameListItem({ game }: { game: GameModel }) {
  return (
    <Link
      to={`/games/${game.id}`}
      className="flex flex-col gap-1 rounded-lg border border-border p-3 text-sm transition-colors hover:bg-muted"
    >
      <span className="font-medium text-dark-grey">{formatGameDateTime(game.startTime)}</span>
      <span className="flex items-center justify-between gap-2 text-light-grey">
        <span className="truncate">{game.location ?? 'Location TBC'}</span>
        <GameStatusBadge status={game.status} />
      </span>
      {game.organiser && (
        <span className="text-xs text-light-grey">Organised by @{game.organiser.tag}</span>
      )}
    </Link>
  )
}
