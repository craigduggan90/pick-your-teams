import { apiFetch } from './client'

export interface InvitationGameModel {
  id: string
  startTime: string
  duration: number
  location: string | null
}

export interface InvitationOrganiserModel {
  id: string
  tag: string
  displayName: string
}

// Same shape as InvitationOrganiserModel, kept as a distinct type to match the backend's own
// InvitationInviteeModel — nullable because the domain model's User navigation is nullable
// end-to-end, though every current invitation resolves a real user (tag-only invites, no
// email-only fallback).
export interface InvitationInviteeModel {
  id: string
  tag: string
  displayName: string
}

export type InvitationStatus = 'Open' | 'Accepted' | 'Declined' | 'Failed'

export interface InvitationModel {
  id: string
  status: InvitationStatus
  game: InvitationGameModel
  organiser: InvitationOrganiserModel | null
  invitee: InvitationInviteeModel | null
}

export interface InvitationsPage {
  data: InvitationModel[]
  cursor: string | null
  count: number
}

export interface GetInvitationsParams {
  gameId?: string
  userId?: string
  status?: InvitationStatus
  pageSize?: number
  cursor?: string
}

// PascalCase to match GetInvitationsRequestModel's bound property names directly, same
// query-binding convention as api/games.ts's toQueryString.
function toQueryString(params: GetInvitationsParams): string {
  const searchParams = new URLSearchParams()
  const entries: [string, string | number | undefined][] = [
    ['GameId', params.gameId],
    ['UserId', params.userId],
    ['Status', params.status],
    ['PageSize', params.pageSize],
    ['Cursor', params.cursor],
  ]
  for (const [key, value] of entries) {
    if (value !== undefined) {
      searchParams.set(key, String(value))
    }
  }
  const query = searchParams.toString()
  return query ? `?${query}` : ''
}

export function getInvitations(params: GetInvitationsParams, token: string): Promise<InvitationsPage> {
  return apiFetch<InvitationsPage>(`/v1/invitations${toQueryString(params)}`, { token })
}

export interface CreateInvitationsRequestModel {
  GameId: string
  UserTags: string[]
}

export function createInvitations(body: CreateInvitationsRequestModel, token: string): Promise<void> {
  return apiFetch<void>('/v1/invitations', { token, method: 'POST', body })
}

export function acceptInvitation(id: string, token: string): Promise<void> {
  return apiFetch<void>(`/v1/invitations/${id}`, { token, method: 'POST' })
}

export function declineInvitation(id: string, token: string): Promise<void> {
  return apiFetch<void>(`/v1/invitations/${id}`, { token, method: 'DELETE' })
}
