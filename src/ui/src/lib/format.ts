// No timezone/locale app setting yet (see 02-games-list.png's "Future: App setting" note) —
// everything renders in UTC for now, matching the diagram's "(UTC)" suffix.
const UTC = 'UTC'

function ordinal(day: number): string {
  if (day >= 11 && day <= 13) {
    return `${day}th`
  }
  switch (day % 10) {
    case 1:
      return `${day}st`
    case 2:
      return `${day}nd`
    case 3:
      return `${day}rd`
    default:
      return `${day}th`
  }
}

/** "Monday 10th August @ 20:00 (UTC)", matching 02-games-list.png's row format. */
export function formatGameDateTime(iso: string): string {
  const date = new Date(iso)
  const weekday = new Intl.DateTimeFormat('en-GB', { weekday: 'long', timeZone: UTC }).format(date)
  const month = new Intl.DateTimeFormat('en-GB', { month: 'long', timeZone: UTC }).format(date)
  const day = Number(new Intl.DateTimeFormat('en-GB', { day: 'numeric', timeZone: UTC }).format(date))
  const time = new Intl.DateTimeFormat('en-GB', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
    timeZone: UTC,
  }).format(date)
  return `${weekday} ${ordinal(day)} ${month} @ ${time} (UTC)`
}

/** "20:00", for use inside a datetime-local input's date portion. */
export function toDateTimeLocalValue(iso: string): string {
  const date = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getUTCFullYear()}-${pad(date.getUTCMonth() + 1)}-${pad(date.getUTCDate())}T${pad(date.getUTCHours())}:${pad(date.getUTCMinutes())}`
}

/** Inverse of toDateTimeLocalValue — treats the naive "YYYY-MM-DDTHH:mm" value as UTC. */
export function fromDateTimeLocalValue(value: string): string {
  return `${value}:00.000Z`
}

/** The top of the next hour (UTC), as a datetime-local value — the sensible default for a new
 * game's start time rather than leaving the field blank. */
export function nextHourStart(): string {
  const date = new Date()
  date.setUTCMinutes(0, 0, 0)
  date.setUTCHours(date.getUTCHours() + 1)
  return toDateTimeLocalValue(date.toISOString())
}

/** "2026-08-20", for use inside a plain date input. */
export function toDateValue(iso: string): string {
  return toDateTimeLocalValue(iso).slice(0, 10)
}

/** Inverse of toDateValue — treats the naive "YYYY-MM-DD" value as UTC midnight. */
export function fromDateValue(value: string): string {
  return `${value}T00:00:00.000Z`
}

/** Rolls an ISO day-boundary forward by one day — for turning a picked "end date" into an
 * exclusive upper bound that still includes everything on that date (e.g. a "Game Start To" of
 * 3rd September should include games starting any time on the 3rd, not just at 00:00). */
export function nextDayBoundary(iso: string): string {
  const date = new Date(iso)
  date.setUTCDate(date.getUTCDate() + 1)
  return date.toISOString()
}
