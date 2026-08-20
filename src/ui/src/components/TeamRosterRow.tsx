import { SelectField, type SelectOption } from '@/components/Select'
import { cn } from '@/lib/utils'
import type { GameTeamPlayerModel } from '@/api/games'

export type RosterTeam = 'Home' | 'Away' | 'None'

// Synthetic value routed to onRemove instead of a team assignment — GameTeamEnum has no "remove
// from game" member, this is never sent to the API as a team.
const REMOVE_FROM_GAME = 'RemoveFromGame'
type RowActionValue = RosterTeam | typeof REMOVE_FROM_GAME

const TEAM_LABELS: Record<RosterTeam, string> = {
  Home: 'Home',
  Away: 'Away',
  None: 'Unassigned',
}

function rowColorClasses(team: RosterTeam, isPending: boolean): string {
  if (team === 'Home') {
    return isPending ? 'border-primary/30 bg-primary/5' : 'border-primary bg-primary/10'
  }
  if (team === 'Away') {
    return isPending ? 'border-secondary/30 bg-secondary/5' : 'border-secondary bg-secondary/10'
  }
  return 'border-border bg-background'
}

// Current-state-aware per claude.md: a Home player is offered Away/Unassign/Remove, never "Home"
// itself — the row's own team label (rendered separately, not via the select) already shows the
// current state.
function buildOptions(team: RosterTeam): SelectOption<RowActionValue>[] {
  const options: SelectOption<RowActionValue>[] = []
  if (team !== 'Home') options.push({ value: 'Home', label: 'To Home Team' })
  if (team !== 'Away') options.push({ value: 'Away', label: 'To Away Team' })
  if (team !== 'None') options.push({ value: 'None', label: 'Remove from Team' })
  options.push({ value: REMOVE_FROM_GAME, label: 'Remove from Game', destructive: true })
  return options
}

export interface TeamRosterRowProps {
  player: GameTeamPlayerModel
  team: RosterTeam
  editable?: boolean
  isPending?: boolean
  onTeamChange?: (playerId: string, team: RosterTeam) => void
  onRemove?: (player: GameTeamPlayerModel) => void
}

export function TeamRosterRow({
  player,
  team,
  editable = false,
  isPending = false,
  onTeamChange,
  onRemove,
}: TeamRosterRowProps) {
  return (
    <div
      data-testid={`team-roster-row-${player.id}`}
      className={cn(
        'flex items-center justify-between gap-2 rounded-lg border p-2.5 text-sm',
        rowColorClasses(team, editable && isPending),
      )}
    >
      <div className="min-w-0">
        <p className="truncate font-medium text-dark-grey">
          {player.displayName ?? 'Unknown player'}
          {player.tag && <span className="font-normal text-light-grey"> (@{player.tag})</span>}
        </p>
        <p className="text-xs text-light-grey">Rating {player.rating}</p>
      </div>
      <div className="flex shrink-0 items-center gap-2">
        <span className="text-xs font-medium text-light-grey">{TEAM_LABELS[team]}</span>
        {editable && (
          // Deliberately uncontrolled (no `value` prop) — it's used as an action menu, not a
          // persistent field. Rows live in a different section's list per pending team, so a
          // team-changing pick remounts this row under its new section, which resets the select
          // back to its placeholder for free; no manual reset needed.
          <SelectField<RowActionValue>
            placeholder="Actions"
            options={buildOptions(team)}
            onValueChange={(value) => {
              if (value === REMOVE_FROM_GAME) {
                onRemove?.(player)
              } else {
                onTeamChange?.(player.id, value)
              }
            }}
            className="h-9 w-36 text-sm"
          />
        )}
      </div>
    </div>
  )
}
