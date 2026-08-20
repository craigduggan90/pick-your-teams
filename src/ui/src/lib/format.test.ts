import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  formatGameDateTime,
  fromDateTimeLocalValue,
  fromDateValue,
  nextDayBoundary,
  nextHourStart,
  toDateTimeLocalValue,
  toDateValue,
} from './format'

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

describe('date round trip', () => {
  it('converts an ISO string to a plain date value and back to UTC midnight', () => {
    const iso = '2026-08-10T20:00:00.000Z'
    expect(toDateValue(iso)).toBe('2026-08-10')
    expect(fromDateValue('2026-08-10')).toBe('2026-08-10T00:00:00.000Z')
  })
})

describe('nextDayBoundary', () => {
  it('rolls a day boundary forward by one day', () => {
    expect(nextDayBoundary('2026-08-10T00:00:00.000Z')).toBe('2026-08-11T00:00:00.000Z')
  })

  it('rolls over the month/year when needed', () => {
    expect(nextDayBoundary('2026-12-31T00:00:00.000Z')).toBe('2027-01-01T00:00:00.000Z')
  })
})

describe('nextHourStart', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('rounds up to the top of the next hour', () => {
    vi.setSystemTime(new Date('2026-08-10T20:14:32.000Z'))
    expect(nextHourStart()).toBe('2026-08-10T21:00')
  })

  it('rolls over the day when already at the last hour', () => {
    vi.setSystemTime(new Date('2026-08-10T23:45:00.000Z'))
    expect(nextHourStart()).toBe('2026-08-11T00:00')
  })
})
