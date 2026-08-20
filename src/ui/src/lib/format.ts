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
