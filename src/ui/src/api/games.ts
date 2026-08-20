import { apiFetch } from './client'

export interface GameOrganiserModel {
  id: string
  tag: string
  displayName: string
}

export type GameStatus = 'Scheduled' | 'Finished'

// "None" is a draw, not "no result yet" — the API only ever returns/accepts this once a result
// has been recorded (see RecordResultRequestModelExample.NoWinnerExample, labeled "Draw").
export type GameWinner = 'Home' | 'Away' | 'None'

export interface GameModel {
  id: string
  location: string | null
  startTime: string
  duration: number
  teamSize: number
  status: GameStatus
  organiser: GameOrganiserModel | null
}

export interface GameDetailModel extends GameModel {
  winner: GameWinner | null
  homeTeamRating: number | null
  awayTeamRating: number | null
  created: string
  modified: string
}

export interface GamesPage {
  data: GameModel[]
  cursor: string | null
  count: number
}

export interface GetGamesParams {
  startTimeFrom?: string
  startTimeTo?: string
  teamSize?: number
  status?: GameStatus
  pageSize?: number
  cursor?: string
}

// PascalCase to match GetGamesRequestModel's bound property names directly — ASP.NET Core's
// query binding is case-insensitive on input, same rule as request bodies (see api/users.ts).
function toQueryString(params: GetGamesParams): string {
  const searchParams = new URLSearchParams()
  const entries: [string, string | number | undefined][] = [
    ['StartTimeFrom', params.startTimeFrom],
    ['StartTimeTo', params.startTimeTo],
    ['TeamSize', params.teamSize],
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

export function getGames(params: GetGamesParams, token: string): Promise<GamesPage> {
  return apiFetch<GamesPage>(`/v1/games${toQueryString(params)}`, { token })
}

export function getGameById(id: string, token: string): Promise<GameDetailModel> {
  return apiFetch<GameDetailModel>(`/v1/games/${id}`, { token })
}

export interface CreateGameRequestModel {
  Location?: string
  StartTime: string
  Duration: number
  TeamSize: number
  OrganiserId: string
}

export function createGame(body: CreateGameRequestModel, token: string): Promise<GameModel> {
  return apiFetch<GameModel>('/v1/games', { token, method: 'POST', body })
}

// Request body stays PascalCase, matching UpdateGameRequestModel directly (TeamSize isn't part
// of it — team size can't be changed after a game is created).
export interface UpdateGameRequestModel {
  Location?: string | null
  StartTime?: string
  Duration?: number
}

export function updateGame(
  id: string,
  body: UpdateGameRequestModel,
  token: string,
): Promise<void> {
  return apiFetch<void>(`/v1/games/${id}`, { token, method: 'PATCH', body })
}

export function deleteGame(id: string, token: string): Promise<void> {
  return apiFetch<void>(`/v1/games/${id}`, { token, method: 'DELETE' })
}

export function recordResult(id: string, winner: GameWinner, token: string): Promise<void> {
  return apiFetch<void>(`/v1/games/${id}/result`, {
    token,
    method: 'POST',
    body: { Winner: winner },
  })
}

// Tag is nullable because a Dummy player has no linked User to pull it from.
export interface GameTeamPlayerModel {
  id: string
  displayName: string | null
  tag: string | null
  rating: number
}

export interface GameTeamModel {
  players: GameTeamPlayerModel[]
  teamRating: number
}

export interface GameTeamsModel {
  id: string
  home: GameTeamModel | null
  away: GameTeamModel | null
  unassigned: GameTeamPlayerModel[]
}

export function getGameTeams(id: string, token: string): Promise<GameTeamsModel> {
  return apiFetch<GameTeamsModel>(`/v1/games/${id}/teams`, { token })
}

export interface SetTeamsRequestModel {
  HomeTeamIds: string[]
  AwayTeamIds: string[]
}

export function setGameTeams(
  id: string,
  body: SetTeamsRequestModel,
  token: string,
): Promise<void> {
  return apiFetch<void>(`/v1/games/${id}/teams`, { token, method: 'PUT', body })
}

export interface GenerateTeamsRequestModel {
  HomeTeamSeedIds: string[]
  AwayTeamSeedIds: string[]
  Differential: number
}

export function generateGameTeams(
  id: string,
  body: GenerateTeamsRequestModel,
  token: string,
): Promise<GameTeamsModel> {
  return apiFetch<GameTeamsModel>(`/v1/games/${id}/teams/generate`, { token, method: 'POST', body })
}
