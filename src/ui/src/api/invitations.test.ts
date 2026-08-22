import { beforeEach, describe, expect, it, vi } from 'vitest'
import { apiFetch } from './client'
import { acceptInvitation, createInvitations, declineInvitation, getInvitations } from './invitations'

vi.mock('./client', () => ({
  apiFetch: vi.fn(),
}))

describe('invitations api', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('getInvitations fetches with no query string when no filters are given', async () => {
    vi.mocked(apiFetch).mockResolvedValue({ data: [], cursor: null, count: 0 })

    await getInvitations({}, 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/invitations', { token: 'token123' })
  })

  it('getInvitations builds a PascalCase query string from the given filters', async () => {
    vi.mocked(apiFetch).mockResolvedValue({ data: [], cursor: null, count: 0 })

    await getInvitations({ userId: 'user-1', status: 'Open', pageSize: 20, cursor: 'abc' }, 'token123')

    const [path] = vi.mocked(apiFetch).mock.calls[0]
    expect(path).toContain('/v1/invitations?')
    const query = new URLSearchParams(path.split('?')[1])
    expect(query.get('UserId')).toBe('user-1')
    expect(query.get('Status')).toBe('Open')
    expect(query.get('PageSize')).toBe('20')
    expect(query.get('Cursor')).toBe('abc')
  })

  it('createInvitations POSTs the game id and tags', async () => {
    vi.mocked(apiFetch).mockResolvedValue(undefined)

    await createInvitations({ GameId: 'game-1', UserTags: ['bob', 'alice'] }, 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/invitations', {
      token: 'token123',
      method: 'POST',
      body: { GameId: 'game-1', UserTags: ['bob', 'alice'] },
    })
  })

  it('acceptInvitation POSTs to the invitation id', async () => {
    vi.mocked(apiFetch).mockResolvedValue(undefined)

    await acceptInvitation('inv-1', 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/invitations/inv-1', {
      token: 'token123',
      method: 'POST',
    })
  })

  it('declineInvitation DELETEs the invitation id', async () => {
    vi.mocked(apiFetch).mockResolvedValue(undefined)

    await declineInvitation('inv-1', 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/invitations/inv-1', {
      token: 'token123',
      method: 'DELETE',
    })
  })
})
