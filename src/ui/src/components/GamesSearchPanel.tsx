import { useEffect, useState } from 'react'
import { Button } from '@/components/Button'
import { TextInput } from '@/components/TextInput'
import { cn } from '@/lib/utils'
import { toDateTimeLocalValue, fromDateTimeLocalValue } from '@/lib/format'
import type { GameStatus } from '@/api/games'

export interface GamesSearchFilters {
  startTimeFrom?: string
  startTimeTo?: string
  teamSize?: number
  status?: GameStatus
}

export interface GamesSearchPanelProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  filters: GamesSearchFilters
  onApply: (filters: GamesSearchFilters) => void
}

function defaultStartTimeFrom(): string {
  const date = new Date()
  date.setUTCHours(0, 0, 0, 0)
  return date.toISOString()
}

function defaultStartTimeTo(from: string | undefined): string {
  const base = from ? new Date(from) : new Date(defaultStartTimeFrom())
  const date = new Date(base)
  date.setUTCDate(date.getUTCDate() + 7)
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

export function GamesSearchPanel({ open, onOpenChange, filters, onApply }: GamesSearchPanelProps) {
  // "Games I'm In" / "Games I've Organised" is visual only — GetGamesQuery doesn't expose
  // OrganiserId/UserId yet (see docs/claude/stage-3.md), so this toggle can't actually filter.
  const [organiserOnly, setOrganiserOnly] = useState(false)
  const [startFromEnabled, setStartFromEnabled] = useState(Boolean(filters.startTimeFrom))
  const [startFrom, setStartFrom] = useState(filters.startTimeFrom ?? defaultStartTimeFrom())
  const [startToEnabled, setStartToEnabled] = useState(Boolean(filters.startTimeTo))
  const [startTo, setStartTo] = useState(filters.startTimeTo ?? defaultStartTimeTo(filters.startTimeFrom))
  const [teamSize, setTeamSize] = useState(filters.teamSize?.toString() ?? '')
  const [status, setStatus] = useState<GameStatus | undefined>(filters.status)

  useEffect(() => {
    if (!open) {
      return
    }
    setOrganiserOnly(false)
    setStartFromEnabled(Boolean(filters.startTimeFrom))
    setStartFrom(filters.startTimeFrom ?? defaultStartTimeFrom())
    setStartToEnabled(Boolean(filters.startTimeTo))
    setStartTo(filters.startTimeTo ?? defaultStartTimeTo(filters.startTimeFrom))
    setTeamSize(filters.teamSize?.toString() ?? '')
    setStatus(filters.status)
    // Intentionally re-seeds the draft only when `open` flips true — not on every `filters` or
    // draft-field change, which would fight the user's in-progress edits.
  }, [open]) // eslint-disable-line react-hooks/exhaustive-deps

  if (!open) {
    return null
  }

  const handleApply = () => {
    onApply({
      startTimeFrom: startFromEnabled ? startFrom : undefined,
      startTimeTo: startToEnabled ? startTo : undefined,
      teamSize: teamSize ? Number(teamSize) : undefined,
      status,
    })
    onOpenChange(false)
  }

  return (
    <div className="fixed inset-0 z-50 flex flex-col bg-background">
      <header className="flex items-center gap-3 bg-primary px-4 py-3 text-primary-foreground">
        <h1 className="flex-1 text-lg font-medium">Games / Search</h1>
      </header>

      <div className="flex flex-1 flex-col gap-6 overflow-y-auto p-4">
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
          <div className="flex items-end gap-2">
            <label className="flex items-center gap-2 text-sm text-dark-grey">
              <input
                type="checkbox"
                checked={startFromEnabled}
                onChange={(event) => setStartFromEnabled(event.target.checked)}
              />
              Game Start From
            </label>
            {startFromEnabled && (
              <input
                type="datetime-local"
                value={toDateTimeLocalValue(startFrom)}
                onChange={(event) => setStartFrom(fromDateTimeLocalValue(event.target.value))}
                className="h-10 flex-1 rounded-lg border border-input bg-background px-2 text-sm"
              />
            )}
          </div>

          <div className="flex items-end gap-2">
            <label className="flex items-center gap-2 text-sm text-dark-grey">
              <input
                type="checkbox"
                checked={startToEnabled}
                onChange={(event) => setStartToEnabled(event.target.checked)}
              />
              Game Start To
            </label>
            {startToEnabled && (
              <input
                type="datetime-local"
                value={toDateTimeLocalValue(startTo)}
                onChange={(event) => setStartTo(fromDateTimeLocalValue(event.target.value))}
                className="h-10 flex-1 rounded-lg border border-input bg-background px-2 text-sm"
              />
            )}
          </div>

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

      <div className="flex justify-end gap-2 border-t border-border bg-background p-4">
        <Button variant="outline" onClick={() => onOpenChange(false)}>
          Cancel
        </Button>
        <Button variant="primary" onClick={handleApply}>
          Apply
        </Button>
      </div>
    </div>
  )
}
