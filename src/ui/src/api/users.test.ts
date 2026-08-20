import { describe, expect, it, vi } from 'vitest'
import { apiFetch } from './client'
import { deleteUser, getSelf, updateUser } from './users'

vi.mock('./client', () => ({
  apiFetch: vi.fn(),
}))

describe('users api', () => {
  it('getSelf fetches the current user', async () => {
    vi.mocked(apiFetch).mockResolvedValue({ id: '1', tag: 'bob' })

    const result = await getSelf('token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/users/self', { token: 'token123' })
    expect(result).toEqual({ id: '1', tag: 'bob' })
  })

  it('updateUser PATCHes the given id with the body', async () => {
    vi.mocked(apiFetch).mockResolvedValue(undefined)

    await updateUser('1', { Tag: 'bob' }, 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/users/1', {
      token: 'token123',
      method: 'PATCH',
      body: { Tag: 'bob' },
    })
  })

  it('deleteUser DELETEs the given id', async () => {
    vi.mocked(apiFetch).mockResolvedValue(undefined)

    await deleteUser('1', 'token123')

    expect(apiFetch).toHaveBeenCalledWith('/v1/users/1', {
      token: 'token123',
      method: 'DELETE',
    })
  })
})
