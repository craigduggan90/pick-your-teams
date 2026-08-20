import { beforeEach, describe, expect, it, vi } from 'vitest'
import { apiFetch } from './client'
import { createGame, deleteGame, getGameById, getGames, recordResult, updateGame } from './games'

vi.mock('./client', () => ({
  apiFetch: vi.fn(),
}))

describe('games api', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('getGames fetches with no query string when no filters are given', async () => {
    vi.mocked(apiFetch).mockResolvedValue({ data: [], cursor: null, count: 0 })

    await getGames({}, 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/games', { token: 'token123' })
  })

  it('getGames builds a PascalCase query string from the given filters', async () => {
    vi.mocked(apiFetch).mockResolvedValue({ data: [], cursor: null, count: 0 })

    await getGames({ status: 'Scheduled', teamSize: 5, pageSize: 20, cursor: 'abc' }, 'token123')

    const [path] = vi.mocked(apiFetch).mock.calls[0]
    expect(path).toContain('/v1/games?')
    const query = new URLSearchParams(path.split('?')[1])
    expect(query.get('Status')).toBe('Scheduled')
    expect(query.get('TeamSize')).toBe('5')
    expect(query.get('PageSize')).toBe('20')
    expect(query.get('Cursor')).toBe('abc')
  })

  it('createGame POSTs the body and returns the created game', async () => {
    vi.mocked(apiFetch).mockResolvedValue({ id: 'game-1' })

    const result = await createGame(
      { StartTime: '2026-08-10T20:00:00.000Z', Duration: 60, TeamSize: 5, OrganiserId: 'user-1' },
      'token123',
    )

    expect(apiFetch).toHaveBeenCalledWith('/v1/games', {
      token: 'token123',
      method: 'POST',
      body: { StartTime: '2026-08-10T20:00:00.000Z', Duration: 60, TeamSize: 5, OrganiserId: 'user-1' },
    })
    expect(result).toEqual({ id: 'game-1' })
  })

  it('getGameById fetches the game detail', async () => {
    vi.mocked(apiFetch).mockResolvedValue({ id: '1' })

    const result = await getGameById('1', 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/games/1', { token: 'token123' })
    expect(result).toEqual({ id: '1' })
  })

  it('updateGame PATCHes the given id with the body', async () => {
    vi.mocked(apiFetch).mockResolvedValue(undefined)

    await updateGame('1', { Location: 'The Pitch' }, 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/games/1', {
      token: 'token123',
      method: 'PATCH',
      body: { Location: 'The Pitch' },
    })
  })

  it('deleteGame DELETEs the given id', async () => {
    vi.mocked(apiFetch).mockResolvedValue(undefined)

    await deleteGame('1', 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/games/1', {
      token: 'token123',
      method: 'DELETE',
    })
  })

  it('recordResult POSTs the winner', async () => {
    vi.mocked(apiFetch).mockResolvedValue(undefined)

    await recordResult('1', 'None', 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/games/1/result', {
      token: 'token123',
      method: 'POST',
      body: { Winner: 'None' },
    })
  })
})
