import { describe, expect, it } from 'vitest'
import { formatGameDateTime, fromDateTimeLocalValue, toDateTimeLocalValue } from './format'

describe('formatGameDateTime', () => {
  it('formats with weekday, ordinal day, month, 24h time, and (UTC)', () => {
    expect(formatGameDateTime('2026-08-10T20:00:00.000Z')).toBe('Monday 10th August @ 20:00 (UTC)')
  })

  it.each([
    ['2026-08-01T00:00:00.000Z', '1st'],
    ['2026-08-02T00:00:00.000Z', '2nd'],
    ['2026-08-03T00:00:00.000Z', '3rd'],
    ['2026-08-11T00:00:00.000Z', '11th'],
    ['2026-08-12T00:00:00.000Z', '12th'],
    ['2026-08-13T00:00:00.000Z', '13th'],
    ['2026-08-21T00:00:00.000Z', '21st'],
  ])('applies the correct ordinal suffix for %s', (iso, expected) => {
    expect(formatGameDateTime(iso)).toContain(expected)
  })
})

describe('datetime-local round trip', () => {
  it('converts an ISO string to a datetime-local value and back', () => {
    const iso = '2026-08-10T20:00:00.000Z'
    const local = toDateTimeLocalValue(iso)
    expect(local).toBe('2026-08-10T20:00')
    expect(fromDateTimeLocalValue(local)).toBe(iso)
  })
})
