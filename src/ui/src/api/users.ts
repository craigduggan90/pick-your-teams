import { apiFetch } from './client'

export interface UserDetailModel {
  Id: string
  Tag: string
  DisplayName: string
  Rating: number
  Email: string
  Mobile: string | null
  Created: string
  Modified: string
}

export interface UpdateUserRequestModel {
  Tag?: string
  DisplayName?: string
  Email?: string
  Mobile?: string
}

export function getSelf(token: string): Promise<UserDetailModel> {
  return apiFetch<UserDetailModel>('/v1/users/self', { token })
}

export function updateUser(
  id: string,
  body: UpdateUserRequestModel,
  token: string,
): Promise<void> {
  return apiFetch<void>(`/v1/users/${id}`, { token, method: 'PATCH', body })
}
