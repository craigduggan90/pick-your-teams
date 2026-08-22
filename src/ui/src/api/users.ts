import { apiFetch } from './client'

// The API serializes responses in camelCase (ASP.NET's default JSON naming policy) even though
// the C# model properties are PascalCase — this must match the wire format, not the C# source.
export interface UserDetailModel {
  id: string
  tag: string
  displayName: string
  rating: number
  email: string
  mobile: string | null
  created: string
  modified: string
  pendingInvitations: number
}

// Request bodies stay PascalCase, matching the C# request DTO directly — ASP.NET Core's model
// binding is case-insensitive on input, so this works and there's no camelCase requirement here.
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

export function deleteUser(id: string, token: string): Promise<void> {
  return apiFetch<void>(`/v1/users/${id}`, { token, method: 'DELETE' })
}
