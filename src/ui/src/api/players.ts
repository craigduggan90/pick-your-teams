import { apiFetch } from './client'

export interface PlayerModel {
  id: string
  gameId: string
  userId: string | null
  tag: string | null
  type: 'User' | 'Dummy'
  displayName: string | null
  rating: number
  team: 'None' | 'Home' | 'Away'
}

export interface CreateDummyPlayerRequestModel {
  GameId: string
  DisplayName: string
  EstimatedRating: number
}

export function createDummyPlayer(
  body: CreateDummyPlayerRequestModel,
  token: string,
): Promise<PlayerModel> {
  return apiFetch<PlayerModel>('/v1/players/dummy', { token, method: 'POST', body })
}

export function deletePlayer(id: string, token: string): Promise<void> {
  return apiFetch<void>(`/v1/players/${id}`, { token, method: 'DELETE' })
}
