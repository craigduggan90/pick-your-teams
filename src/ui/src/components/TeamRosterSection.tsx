import type { ReactNode } from 'react'
import { TeamRosterRow, type RosterTeam } from '@/components/TeamRosterRow'
import { cn } from '@/lib/utils'
import type { GameTeamPlayerModel } from '@/api/games'

const HEADING_CLASSES: Record<RosterTeam, string> = {
  Home: 'text-primary',
  Away: 'text-secondary',
  None: 'text-dark-grey',
}

const TITLES: Record<RosterTeam, string> = {
  Home: 'Home Team',
  Away: 'Away Team',
  None: 'Unassigned',
}

// Tag (A-Z, nulls last) -> Display Name (A-Z) -> Rating (descending), per live feedback.
function comparePlayers(a: GameTeamPlayerModel, b: GameTeamPlayerModel): number {
  if (a.tag !== b.tag) {
    if (a.tag === null) return 1
    if (b.tag === null) return -1
    return a.tag.localeCompare(b.tag)
  }
  const nameCompare = (a.displayName ?? '').localeCompare(b.displayName ?? '')
  if (nameCompare !== 0) return nameCompare
  return b.rating - a.rating
}

export interface TeamRosterSectionProps {
  team: RosterTeam
  players: GameTeamPlayerModel[]
  rating?: number
  pendingPlayerIds?: Set<string>
  editable?: boolean
  onTeamChange?: (playerId: string, team: RosterTeam) => void
  onRemove?: (player: GameTeamPlayerModel) => void
  /** Rendered between the heading and the player rows — e.g. Unassigned's Invite Players / Add
   * Non-User Player actions, which live at the top of that section per live feedback. */
  topContent?: ReactNode
}

export function TeamRosterSection({
  team,
  players,
  rating,
  pendingPlayerIds,
  editable = false,
  onTeamChange,
  onRemove,
  topContent,
}: TeamRosterSectionProps) {
  const sortedPlayers = [...players].sort(comparePlayers)

  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-baseline justify-between">
        <h2 className={cn('font-semibold', HEADING_CLASSES[team])}>{TITLES[team]}</h2>
        {rating !== undefined && (
          <span className={cn('text-sm font-medium', HEADING_CLASSES[team])}>{rating}</span>
        )}
      </div>
      {topContent}
      {sortedPlayers.length === 0 ? (
        <p className="text-sm text-light-grey">No players.</p>
      ) : (
        <div className="flex flex-col gap-2">
          {sortedPlayers.map((player) => (
            <TeamRosterRow
              key={player.id}
              player={player}
              team={team}
              editable={editable}
              isPending={pendingPlayerIds?.has(player.id)}
              onTeamChange={onTeamChange}
              onRemove={onRemove}
            />
          ))}
        </div>
      )}
    </div>
  )
}
