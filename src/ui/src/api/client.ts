import type { ProblemDetails } from './types'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export class ApiError extends Error {
  status: number
  problem: ProblemDetails

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? `Request failed with status ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

export interface ApiRequestOptions {
  token: string
  method?: string
  body?: unknown
  signal?: AbortSignal
}

export async function apiFetch<T>(
  path: string,
  { token, method = 'GET', body, signal }: ApiRequestOptions,
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    signal,
    headers: {
      Authorization: `Bearer ${token}`,
      ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })

  if (!response.ok) {
    const problem: ProblemDetails = await response.json().catch(() => ({}))
    throw new ApiError(response.status, problem)
  }

  // Not every empty-body success response is a 204 — e.g. CreateInvitations returns a bare
  // StatusCode(201) with no body. Parsing an empty body as JSON throws a SyntaxError, so check
  // the actual body instead of assuming only 204 is body-less.
  const text = await response.text()
  if (!text) {
    return undefined as T
  }

  return JSON.parse(text) as T
}
