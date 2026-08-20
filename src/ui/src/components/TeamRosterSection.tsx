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

export interface TeamRosterSectionProps {
  team: RosterTeam
  players: GameTeamPlayerModel[]
  rating?: number
  pendingPlayerIds?: Set<string>
  editable?: boolean
  onTeamChange?: (playerId: string, team: RosterTeam) => void
  onRemove?: (player: GameTeamPlayerModel) => void
}

export function TeamRosterSection({
  team,
  players,
  rating,
  pendingPlayerIds,
  editable = false,
  onTeamChange,
  onRemove,
}: TeamRosterSectionProps) {
  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-baseline justify-between">
        <h2 className={cn('font-semibold', HEADING_CLASSES[team])}>{TITLES[team]}</h2>
        {rating !== undefined && (
          <span className={cn('text-sm font-medium', HEADING_CLASSES[team])}>{rating}</span>
        )}
      </div>
      {players.length === 0 ? (
        <p className="text-sm text-light-grey">No players.</p>
      ) : (
        <div className="flex flex-col gap-2">
          {players.map((player) => (
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
