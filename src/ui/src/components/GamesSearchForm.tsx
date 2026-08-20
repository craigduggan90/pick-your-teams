import { useState } from 'react'
import { Button } from '@/components/Button'
import { TextInput } from '@/components/TextInput'
import { cn } from '@/lib/utils'
import { toDateValue, fromDateValue } from '@/lib/format'
import { usePageTitle } from '@/hooks/usePageTitle'
import { usePageFooterActions } from '@/hooks/usePageActions'
import type { GameStatus } from '@/api/games'

export interface GamesSearchFilters {
  // Not sent to the API (GetGamesQuery doesn't expose OrganiserId/UserId yet — see
  // docs/claude/stage-3.md), but still part of the persisted/applied filter state so reopening
  // the search form shows what was last selected, not a reset toggle.
  organiserOnly?: boolean
  startTimeFrom?: string
  startTimeTo?: string
  teamSize?: number
  status?: GameStatus
}

export interface GamesSearchFormProps {
  filters: GamesSearchFilters
  onApply: (filters: GamesSearchFilters) => void
  onCancel: () => void
}

function defaultStartTimeFrom(): string {
  const date = new Date()
  date.setUTCHours(0, 0, 0, 0)
  return date.toISOString()
}

function defaultStartTimeTo(): string {
  const date = new Date()
  date.setUTCDate(date.getUTCDate() + 14)
  date.setUTCHours(0, 0, 0, 0)
  return date.toISOString()
}

function ToggleOption({
  label,
  active,
  onClick,
}: {
  label: string
  active: boolean
  onClick: () => void
}) {
  return (
    <button
      type="button"
      aria-pressed={active}
      onClick={onClick}
      className={cn(
        'flex-1 rounded-lg border px-3 py-2 text-sm font-medium transition-colors',
        active
          ? 'border-info bg-info/10 text-info'
          : 'border-border bg-background text-light-grey hover:bg-muted',
      )}
    >
      {label}
    </button>
  )
}

// Replaces the games list content in place while open — see docs/claude/stage-3.md: this used to
// be a fixed full-screen overlay with its own header/footer stacked on top of the real page
// chrome, which just fought the app's actual Header/Footer. This is a plain page swap instead,
// same as any other routed screen: its own usePageTitle/usePageFooterActions calls, no overlay.
export function GamesSearchForm({ filters, onApply, onCancel }: GamesSearchFormProps) {
  usePageTitle('Games / Search')

  // "Games I'm In" / "Games I've Organised" is visual only — GetGamesQuery doesn't expose
  // OrganiserId/UserId yet (see docs/claude/stage-3.md), so this toggle can't actually filter.
  const [organiserOnly, setOrganiserOnly] = useState(filters.organiserOnly ?? false)
  // Always shown, always defaulted, date-only — nobody's filtering games by time of day, and a
  // native date input already displays its own placeholder mask ("dd/mm/yyyy") even when empty,
  // so an enable/disable checkbox on top of that just adds a step without adding clarity.
  const [startFrom, setStartFrom] = useState(filters.startTimeFrom ?? defaultStartTimeFrom())
  const [startTo, setStartTo] = useState(filters.startTimeTo ?? defaultStartTimeTo())
  const [teamSize, setTeamSize] = useState(filters.teamSize?.toString() ?? '')
  const [status, setStatus] = useState<GameStatus | undefined>(filters.status)

  const handleApply = () => {
    onApply({
      organiserOnly,
      startTimeFrom: startFrom,
      startTimeTo: startTo,
      teamSize: teamSize ? Number(teamSize) : undefined,
      status,
    })
  }

  usePageFooterActions(
    <div className="flex w-full justify-end gap-2 p-4">
      <Button variant="outline" onClick={onCancel}>
        Cancel
      </Button>
      <Button variant="primary" onClick={handleApply}>
        Apply
      </Button>
    </div>,
  )

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col gap-6 p-4">
      <div className="flex gap-2">
        <ToggleOption
          label="Games I'm In"
          active={!organiserOnly}
          onClick={() => setOrganiserOnly(false)}
        />
        <ToggleOption
          label="Games I've Organised"
          active={organiserOnly}
          onClick={() => setOrganiserOnly(true)}
        />
      </div>

      <div className="flex flex-col gap-4">
        <TextInput
          label="Game Start From"
          type="date"
          value={toDateValue(startFrom)}
          onChange={(event) => setStartFrom(fromDateValue(event.target.value))}
        />
        <TextInput
          label="Game Start To"
          type="date"
          value={toDateValue(startTo)}
          onChange={(event) => setStartTo(fromDateValue(event.target.value))}
        />

        <TextInput
          label="Players per Team"
          type="number"
          min={3}
          max={11}
          value={teamSize}
          onChange={(event) => setTeamSize(event.target.value)}
        />
      </div>

      <div className="flex gap-2">
        <ToggleOption
          label="Scheduled"
          active={status === 'Scheduled'}
          onClick={() => setStatus((current) => (current === 'Scheduled' ? undefined : 'Scheduled'))}
        />
        <ToggleOption
          label="Complete"
          active={status === 'Finished'}
          onClick={() => setStatus((current) => (current === 'Finished' ? undefined : 'Finished'))}
        />
      </div>
    </div>
  )
}
