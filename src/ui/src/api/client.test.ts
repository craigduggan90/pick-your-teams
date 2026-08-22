import { afterEach, describe, expect, it, vi } from 'vitest'
import { apiFetch, ApiError } from './client'

describe('apiFetch', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('sends the bearer token and returns the parsed JSON body', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ Id: '1' }), { status: 200 }),
    )
    vi.stubGlobal('fetch', fetchMock)

    const result = await apiFetch<{ Id: string }>('/v1/users/self', { token: 'abc123' })

    expect(result).toEqual({ Id: '1' })
    const [url, init] = fetchMock.mock.calls[0]
    expect(url).toContain('/v1/users/self')
    expect(init.headers.Authorization).toBe('Bearer abc123')
  })

  it('returns undefined for a 204 response', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 204 })))

    const result = await apiFetch('/v1/users/1', { token: 'abc123', method: 'PATCH' })

    expect(result).toBeUndefined()
  })

  // CreateInvitations returns a bare StatusCode(201) with no body — not every empty-body success
  // response is a 204, so this must not be treated as a JSON-parse failure.
  it('returns undefined for a 201 response with no body', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 201 })))

    const result = await apiFetch('/v1/invitations', { token: 'abc123', method: 'POST' })

    expect(result).toBeUndefined()
  })

  it('sends a JSON body and Content-Type when a body is provided', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 204 }))
    vi.stubGlobal('fetch', fetchMock)

    await apiFetch('/v1/users/1', { token: 'abc123', method: 'PATCH', body: { Tag: 'bob' } })

    const [, init] = fetchMock.mock.calls[0]
    expect(init.headers['Content-Type']).toBe('application/json')
    expect(init.body).toBe(JSON.stringify({ Tag: 'bob' }))
  })

  it('throws an ApiError with the parsed problem details on failure', async () => {
    const problem = {
      title: 'Validation Error',
      status: 422,
      errors: { Tag: ["'taken' is not a valid tag."] },
    }
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(new Response(JSON.stringify(problem), { status: 422 })),
    )

    await expect(apiFetch('/v1/users/1', { token: 'abc123' })).rejects.toMatchObject({
      status: 422,
      problem,
    })
  })

  it('throws an ApiError even when the error body is not valid JSON', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('not json', { status: 500 })))

    const error = await apiFetch('/v1/users/1', { token: 'abc123' }).catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    expect((error as ApiError).status).toBe(500)
  })
})
