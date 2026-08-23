import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router'
import { Button } from '@/components/Button'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'
import { TeamRosterSection } from '@/components/TeamRosterSection'
import { RemovePlayerModal } from '@/components/RemovePlayerModal'
import { AddNonUserPlayerForm } from '@/components/AddNonUserPlayerForm'
import { GameDetailsSheet } from '@/components/GameDetailsSheet'
import { toast } from '@/components/Toast'
import { useGame } from '@/hooks/useGame'
import { useSelf } from '@/hooks/useSelf'
import { useGameTeams } from '@/hooks/useGameTeams'
import { useSetGameTeams } from '@/hooks/useSetGameTeams'
import { useGenerateGameTeams } from '@/hooks/useGenerateGameTeams'
import { useCreateDummyPlayer } from '@/hooks/useCreateDummyPlayer'
import { useDeletePlayer } from '@/hooks/useDeletePlayer'
import { usePageTitle } from '@/hooks/usePageTitle'
import { usePageFooterActions } from '@/hooks/usePageActions'
import { ApiError } from '@/api/client'
import type { GameDetailModel, GameTeamPlayerModel, GameTeamsModel } from '@/api/games'
import type { RosterTeam } from '@/components/TeamRosterRow'

export function GameTeamsPage() {
  const { id } = useParams<{ id: string }>()
  const gameQuery = useGame(id)
  const selfQuery = useSelf()
  const teamsQuery = useGameTeams(id)

  const game = gameQuery.data
  const teams = teamsQuery.data

  if (gameQuery.isPending || selfQuery.isPending || teamsQuery.isPending) {
    return <Loading />
  }

  if (gameQuery.isError || teamsQuery.isError || !game || !teams || !id) {
    return <ErrorMessage>Something went wrong loading these teams.</ErrorMessage>
  }

  const isOrganiser = Boolean(
    game.organiser && selfQuery.data && game.organiser.id === selfQuery.data.id,
  )
  const canEdit = isOrganiser && game.status === 'Scheduled'

  return canEdit ? (
    <EditTeamsView gameId={id} game={game} teams={teams} isOrganiser={isOrganiser} />
  ) : (
    <ViewTeamsView gameId={id} game={game} teams={teams} isOrganiser={isOrganiser} />
  )
}

interface TeamsViewProps {
  gameId: string
  game: GameDetailModel
  teams: GameTeamsModel
  isOrganiser: boolean
}

function ViewTeamsView({ gameId, game, teams, isOrganiser }: TeamsViewProps) {
  usePageTitle('Teams')
  const navigate = useNavigate()
  const [detailsOpen, setDetailsOpen] = useState(false)

  usePageFooterActions(
    <div className="flex w-full items-center justify-between gap-2 p-4">
      <Button variant="outline" onClick={() => navigate('/')}>
        Back
      </Button>
      <Button variant="outline" onClick={() => setDetailsOpen(true)}>
        Game Details
      </Button>
    </div>,
  )

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col gap-6 p-4">
      <TeamRosterSection team="Home" players={teams.home?.players ?? []} rating={teams.home?.teamRating} />
      <TeamRosterSection team="Away" players={teams.away?.players ?? []} rating={teams.away?.teamRating} />
      <TeamRosterSection team="None" players={teams.unassigned} />

      <GameDetailsSheet
        open={detailsOpen}
        onOpenChange={setDetailsOpen}
        game={game}
        showManageLink={isOrganiser}
        onManage={() => navigate(`/games/${gameId}`)}
      />
    </div>
  )
}

// Prefers field-level validation messages (e.g. "Game has reached its maximum number of
// players.") over ProblemDetails' generic ValidationProblemDetails title ("One or more
// validation errors occurred.") — that generic title is what `error.message` falls back to for
// any 422 that carries an `errors` dict but no `detail`, which reads as unhelpful noise.
function apiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof ApiError)) return fallback
  const fieldMessages = Object.values(error.problem.errors ?? {}).flat()
  if (fieldMessages.length > 0) return fieldMessages.join(' ')
  return error.problem.detail ?? error.message
}

function EditTeamsView({ gameId, game, teams, isOrganiser }: TeamsViewProps) {
  usePageTitle('Teams')
  const navigate = useNavigate()
  const [detailsOpen, setDetailsOpen] = useState(false)

  // Pending team moves, keyed by player id — not sent to the API until Save. Layering this over
  // the last-fetched server roster (rather than copying the whole roster into local state) is
  // what lets Remove-from-Game/Add-Non-User-Player — both immediate, both triggering a refetch —
  // coexist safely with unsaved moves for *other* players: a refetch changes the base roster
  // without touching this overlay.
  const [overlay, setOverlay] = useState<Record<string, RosterTeam>>({})
  const [removeTarget, setRemoveTarget] = useState<GameTeamPlayerModel | null>(null)
  const [addPlayerFormKey, setAddPlayerFormKey] = useState(0)

  const setTeamsMutation = useSetGameTeams(gameId)
  const generateMutation = useGenerateGameTeams(gameId)
  const createDummyMutation = useCreateDummyPlayer(gameId)
  const deleteMutation = useDeletePlayer(gameId)

  useEffect(() => {
    if (setTeamsMutation.isSuccess) {
      toast.success('Teams saved!')
      setOverlay({})
    }
  }, [setTeamsMutation.isSuccess])

  useEffect(() => {
    if (setTeamsMutation.isError) {
      toast.error(apiErrorMessage(setTeamsMutation.error, 'Something went wrong saving the teams.'))
    }
  }, [setTeamsMutation.isError, setTeamsMutation.error])

  useEffect(() => {
    if (generateMutation.isSuccess && generateMutation.data) {
      const next: Record<string, RosterTeam> = {}
      for (const player of generateMutation.data.home?.players ?? []) next[player.id] = 'Home'
      for (const player of generateMutation.data.away?.players ?? []) next[player.id] = 'Away'
      for (const player of generateMutation.data.unassigned) next[player.id] = 'None'
      setOverlay(next)
    }
  }, [generateMutation.isSuccess, generateMutation.data])

  useEffect(() => {
    if (generateMutation.isError) {
      toast.error(apiErrorMessage(generateMutation.error, 'Something went wrong generating teams.'))
    }
  }, [generateMutation.isError, generateMutation.error])

  useEffect(() => {
    if (deleteMutation.isSuccess) {
      toast.success('Player removed.')
      setRemoveTarget(null)
    }
  }, [deleteMutation.isSuccess])

  useEffect(() => {
    if (deleteMutation.isError) {
      toast.error(apiErrorMessage(deleteMutation.error, 'Something went wrong removing this player.'))
    }
  }, [deleteMutation.isError, deleteMutation.error])

  useEffect(() => {
    if (createDummyMutation.isSuccess) {
      toast.success('Player added!')
      setAddPlayerFormKey((key) => key + 1)
    }
  }, [createDummyMutation.isSuccess])

  useEffect(() => {
    if (createDummyMutation.isError) {
      toast.error(apiErrorMessage(createDummyMutation.error, 'Something went wrong adding this player.'))
    }
  }, [createDummyMutation.isError, createDummyMutation.error])

  const homePlayers: GameTeamPlayerModel[] = []
  const awayPlayers: GameTeamPlayerModel[] = []
  const unassignedPlayers: GameTeamPlayerModel[] = []
  // Pending is a value comparison against the last-*saved* bucket, not just "has an overlay
  // entry" — Generate rebuilds the overlay for every player it returns, including ones whose
  // seeded position didn't actually move, and a presence-only check would show all of them as
  // pending, washing out the distinction entirely.
  const pendingPlayerIds = new Set<string>()
  for (const [savedTeam, players] of [
    ['Home', teams.home?.players ?? []],
    ['Away', teams.away?.players ?? []],
    ['None', teams.unassigned],
  ] as const) {
    for (const player of players) {
      const effectiveTeam = overlay[player.id] ?? savedTeam
      if (effectiveTeam === 'Home') homePlayers.push(player)
      else if (effectiveTeam === 'Away') awayPlayers.push(player)
      else unassignedPlayers.push(player)
      if (effectiveTeam !== savedTeam) pendingPlayerIds.add(player.id)
    }
  }
  // Ratings are recomputed live from the pending roster rather than the server's last-saved
  // TeamRating, so the header numbers actually reflect in-progress moves before Save.
  const homeRating = homePlayers.reduce((sum, player) => sum + player.rating, 0)
  const awayRating = awayPlayers.reduce((sum, player) => sum + player.rating, 0)
  // Team assignment doesn't change the roster's total size (moves, not adds/removes), so this
  // holds regardless of pending overlay state — matches the backend's own Game.MaxPlayers.
  const atCapacity = homePlayers.length + awayPlayers.length + unassignedPlayers.length >= game.teamSize * 2

  function handleTeamChange(playerId: string, team: RosterTeam) {
    setOverlay((prev) => ({ ...prev, [playerId]: team }))
  }

  function handleRemoveRequest(player: GameTeamPlayerModel) {
    if (player.tag) {
      setRemoveTarget(player)
    } else {
      deleteMutation.mutate(player.id)
    }
  }

  const displayNameError =
    createDummyMutation.error instanceof ApiError
      ? createDummyMutation.error.problem.errors?.DisplayName?.[0]
      : undefined
  const ratingError =
    createDummyMutation.error instanceof ApiError
      ? createDummyMutation.error.problem.errors?.EstimatedRating?.[0]
      : undefined

  usePageFooterActions(
    <div className="flex w-full items-center justify-between gap-2 p-4">
      <Button variant="outline" onClick={() => navigate('/')}>
        Back
      </Button>
      <Button variant="outline" onClick={() => setDetailsOpen(true)}>
        Game Details
      </Button>
      <Button
        variant="primary"
        disabled={setTeamsMutation.isPending}
        onClick={() =>
          setTeamsMutation.mutate({
            HomeTeamIds: homePlayers.map((player) => player.id),
            AwayTeamIds: awayPlayers.map((player) => player.id),
          })
        }
      >
        {setTeamsMutation.isPending ? 'Saving…' : 'Save'}
      </Button>
    </div>,
  )

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col gap-6 p-4">
      <div className="flex justify-between gap-2">
        <Button variant="outline" onClick={() => setOverlay({})}>
          Reset
        </Button>
        <Button
          variant="outline"
          disabled={generateMutation.isPending}
          onClick={() =>
            generateMutation.mutate({
              // Seeded from the last-*saved* split, not the pending overlay — a player only
              // counts as fixed once they're actually committed, an unsaved move doesn't
              // un-seed them.
              homeTeamSeedIds: (teams.home?.players ?? []).map((player) => player.id),
              awayTeamSeedIds: (teams.away?.players ?? []).map((player) => player.id),
            })
          }
        >
          {generateMutation.isPending ? 'Generating…' : 'Generate'}
        </Button>
      </div>

      <TeamRosterSection
        team="Home"
        players={homePlayers}
        rating={homeRating}
        pendingPlayerIds={pendingPlayerIds}
        editable
        onTeamChange={handleTeamChange}
        onRemove={handleRemoveRequest}
      />
      <TeamRosterSection
        team="Away"
        players={awayPlayers}
        rating={awayRating}
        pendingPlayerIds={pendingPlayerIds}
        editable
        onTeamChange={handleTeamChange}
        onRemove={handleRemoveRequest}
      />
      <TeamRosterSection
        team="None"
        players={unassignedPlayers}
        pendingPlayerIds={pendingPlayerIds}
        editable
        onTeamChange={handleTeamChange}
        onRemove={handleRemoveRequest}
        topContent={
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" className="flex-1" onClick={() => navigate(`/games/${gameId}/invite`)}>
              Invite Players
            </Button>
            <AddNonUserPlayerForm
              key={addPlayerFormKey}
              onSubmit={(displayName, estimatedRating) =>
                createDummyMutation.mutate({ displayName, estimatedRating })
              }
              isPending={createDummyMutation.isPending}
              displayNameError={displayNameError}
              ratingError={ratingError}
              disabled={atCapacity}
            />
          </div>
        }
      />

      <RemovePlayerModal
        player={removeTarget}
        onOpenChange={(open) => !open && setRemoveTarget(null)}
        onConfirm={(player) => deleteMutation.mutate(player.id)}
        isPending={deleteMutation.isPending}
      />

      <GameDetailsSheet
        open={detailsOpen}
        onOpenChange={setDetailsOpen}
        game={game}
        showManageLink={isOrganiser}
        onManage={() => navigate(`/games/${gameId}`)}
      />
    </div>
  )
}
