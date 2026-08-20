import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router'
import { Button } from '@/components/Button'
import { TextInput } from '@/components/TextInput'
import { GameStatusBadge } from '@/components/GameStatusBadge'
import { Sheet } from '@/components/Sheet'
import { RecordResultModal } from '@/components/RecordResultModal'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'
import { toast } from '@/components/Toast'
import { useGame } from '@/hooks/useGame'
import { useSelf } from '@/hooks/useSelf'
import { useUpdateGame } from '@/hooks/useUpdateGame'
import { useDeleteGame } from '@/hooks/useDeleteGame'
import { useRecordResult } from '@/hooks/useRecordResult'
import { usePageTitle } from '@/hooks/usePageTitle'
import { usePageFooterActions } from '@/hooks/usePageActions'
import { ApiError } from '@/api/client'
import { formatGameWinner, fromDateTimeLocalValue, toDateTimeLocalValue } from '@/lib/format'

export function GameViewPage() {
  usePageTitle('Game')
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const gameQuery = useGame(id)
  const selfQuery = useSelf()
  const updateGame = useUpdateGame(id ?? '')
  const deleteGame = useDeleteGame(id ?? '')
  const recordResult = useRecordResult(id ?? '')

  const [location, setLocation] = useState('')
  const [startTime, setStartTime] = useState('')
  const [duration, setDuration] = useState('')
  const [deleteOpen, setDeleteOpen] = useState(false)
  const [recordResultOpen, setRecordResultOpen] = useState(false)

  const game = gameQuery.data

  useEffect(() => {
    if (game) {
      setLocation(game.location ?? '')
      setStartTime(toDateTimeLocalValue(game.startTime))
      setDuration(String(game.duration))
    }
  }, [game])

  useEffect(() => {
    if (updateGame.isSuccess) {
      toast.success('Changes Saved!')
    }
  }, [updateGame.isSuccess])

  useEffect(() => {
    if (updateGame.isError) {
      const message =
        updateGame.error instanceof ApiError
          ? (updateGame.error.problem.detail ?? updateGame.error.message)
          : 'Something went wrong saving this game.'
      toast.error(message)
    }
  }, [updateGame.isError, updateGame.error])

  useEffect(() => {
    if (deleteGame.isSuccess) {
      toast.success('Game deleted.')
      navigate('/', { replace: true })
    }
  }, [deleteGame.isSuccess, navigate])

  useEffect(() => {
    if (recordResult.isSuccess) {
      toast.success('Result recorded!')
      setRecordResultOpen(false)
    }
  }, [recordResult.isSuccess])

  const isOrganiser = Boolean(
    game?.organiser && selfQuery.data && game.organiser.id === selfQuery.data.id,
  )
  const isScheduled = game?.status === 'Scheduled'
  const canEdit = isOrganiser && isScheduled

  const locationError =
    updateGame.error instanceof ApiError ? updateGame.error.problem.errors?.Location?.[0] : undefined
  const durationError =
    updateGame.error instanceof ApiError ? updateGame.error.problem.errors?.Duration?.[0] : undefined

  usePageFooterActions(
    game && (
      <div className="flex w-full items-center justify-between gap-2 p-4">
        <Button variant="outline" onClick={() => navigate(`/games/${id}/teams`)}>
          Back
        </Button>
        <Button variant="outline" onClick={() => navigate(`/games/${id}/teams`)}>
          {canEdit ? 'Manage Teams' : 'View Teams'}
        </Button>
        {canEdit && (
          <Button
            variant="primary"
            disabled={updateGame.isPending}
            onClick={() =>
              updateGame.mutate({
                Location: location || undefined,
                StartTime: fromDateTimeLocalValue(startTime),
                Duration: Number(duration),
              })
            }
          >
            {updateGame.isPending ? 'Saving…' : 'Save'}
          </Button>
        )}
      </div>
    ),
  )

  if (gameQuery.isPending || selfQuery.isPending) {
    return <Loading />
  }

  if (gameQuery.isError || !game) {
    return <ErrorMessage>Something went wrong loading this game.</ErrorMessage>
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col gap-4 p-4">
      {game.status === 'Finished' && game.winner && (
        <div className="rounded-lg bg-success/10 p-3 text-center font-medium text-success">
          Winner: {formatGameWinner(game.winner)}
        </div>
      )}

      <TextInput
        label="Start Time"
        type="datetime-local"
        value={startTime}
        onChange={(event) => setStartTime(event.target.value)}
        disabled={!canEdit}
      />
      <TextInput
        label="Duration"
        type="number"
        min={15}
        max={120}
        value={duration}
        onChange={(event) => setDuration(event.target.value)}
        error={durationError}
        disabled={!canEdit}
      />
      <TextInput
        label="Location"
        value={location}
        onChange={(event) => setLocation(event.target.value)}
        error={locationError}
        disabled={!canEdit}
      />
      <TextInput label="Players Per Team" value={String(game.teamSize)} disabled />

      <div>
        <p className="mb-1 text-sm text-dark-grey">Status</p>
        <GameStatusBadge status={game.status} className="px-3 py-1.5 text-sm" />
      </div>

      <div className="flex flex-col gap-2">
        {/* Invite Players has no built screen yet (Stage 5) — rendered per the diagram, disabled
            until that stage lands. See docs/claude/stage-3.md. */}
        {isOrganiser && isScheduled && (
          <Button variant="outline" disabled>
            Invite Players
          </Button>
        )}
        {isOrganiser && isScheduled && (
          <Button variant="outline" onClick={() => setRecordResultOpen(true)}>
            Record Result
          </Button>
        )}
        {isOrganiser && (
          <Button variant="destructive" onClick={() => setDeleteOpen(true)}>
            Delete Game
          </Button>
        )}
      </div>

      <Sheet
        open={deleteOpen}
        onOpenChange={setDeleteOpen}
        title="Delete Game?"
        description="This cannot be undone."
        footer={
          <>
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteGame.isPending}
              onClick={() => deleteGame.mutate()}
            >
              {deleteGame.isPending ? 'Deleting…' : 'Delete Game'}
            </Button>
          </>
        }
      />

      <RecordResultModal
        open={recordResultOpen}
        onOpenChange={setRecordResultOpen}
        onConfirm={(winner) => recordResult.mutate(winner)}
        isPending={recordResult.isPending}
      />
    </div>
  )
}
