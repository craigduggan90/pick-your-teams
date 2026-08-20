import { useMutation } from '@tanstack/react-query'
import { useAuth0 } from '@auth0/auth0-react'
import { generateGameTeams, type GameTeamsModel } from '@/api/games'
import type { ApiError } from '@/api/client'

// Fixed for v1 — see docs/claude/stage-4.md's decisions log. Configurable "competitiveness" is
// future scope; no diagram exposes a control for it.
const DIFFERENTIAL = 200

export interface GenerateTeamsVariables {
  homeTeamSeedIds: string[]
  awayTeamSeedIds: string[]
}

// Doesn't invalidate any query — this only returns a suggestion for the caller to fold into
// pending client state. It never touches the server.
export function useGenerateGameTeams(id: string) {
  const { getAccessTokenSilently } = useAuth0()

  return useMutation<GameTeamsModel, ApiError, GenerateTeamsVariables>({
    mutationFn: async ({ homeTeamSeedIds, awayTeamSeedIds }) => {
      const token = await getAccessTokenSilently()
      return generateGameTeams(
        id,
        {
          HomeTeamSeedIds: homeTeamSeedIds,
          AwayTeamSeedIds: awayTeamSeedIds,
          Differential: DIFFERENTIAL,
        },
        token,
      )
    },
  })
}
