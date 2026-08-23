import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router'
import { Button } from '@/components/Button'
import { TextInput } from '@/components/TextInput'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'
import { toast } from '@/components/Toast'
import { useSelf } from '@/hooks/useSelf'
import { useCreateGame } from '@/hooks/useCreateGame'
import { usePageTitle } from '@/hooks/usePageTitle'
import { usePageFooterActions } from '@/hooks/usePageActions'
import { ApiError } from '@/api/client'
import { fromDateTimeLocalValue, nextHourStart } from '@/lib/format'

// No diagram exists for game creation — this is a minimal form
// covering exactly what CreateGameRequestModel needs, styled consistently with the View Game
// fields, so the rest of the games flow (view/edit/record result/delete) can actually be tested
// end to end.
export function NewGamePage() {
  usePageTitle('New Game')
  const navigate = useNavigate()
  const selfQuery = useSelf()
  const createGame = useCreateGame()

  const [location, setLocation] = useState('')
  const [startTime, setStartTime] = useState(nextHourStart)
  const [duration, setDuration] = useState('60')
  const [teamSize, setTeamSize] = useState('5')

  useEffect(() => {
    if (createGame.isSuccess) {
      toast.success('Game created!')
      navigate(`/games/${createGame.data.id}`, { replace: true })
    }
  }, [createGame.isSuccess, createGame.data, navigate])

  useEffect(() => {
    if (createGame.isError) {
      const message =
        createGame.error instanceof ApiError
          ? (createGame.error.problem.detail ?? createGame.error.message)
          : 'Something went wrong creating this game.'
      toast.error(message)
    }
  }, [createGame.isError, createGame.error])

  const locationError =
    createGame.error instanceof ApiError ? createGame.error.problem.errors?.Location?.[0] : undefined
  const durationError =
    createGame.error instanceof ApiError ? createGame.error.problem.errors?.Duration?.[0] : undefined
  const teamSizeError =
    createGame.error instanceof ApiError ? createGame.error.problem.errors?.TeamSize?.[0] : undefined

  const canSubmit = Boolean(startTime && duration && teamSize && selfQuery.data)

  usePageFooterActions(
    <div className="flex w-full justify-between gap-2 p-4">
      <Button variant="outline" onClick={() => navigate('/')}>
        Cancel
      </Button>
      <Button
        variant="primary"
        disabled={!canSubmit || createGame.isPending}
        onClick={() =>
          selfQuery.data &&
          createGame.mutate({
            Location: location || undefined,
            StartTime: fromDateTimeLocalValue(startTime),
            Duration: Number(duration),
            TeamSize: Number(teamSize),
            OrganiserId: selfQuery.data.id,
          })
        }
      >
        {createGame.isPending ? 'Creating…' : 'Create'}
      </Button>
    </div>,
  )

  if (selfQuery.isPending) {
    return <Loading />
  }

  if (selfQuery.isError) {
    return <ErrorMessage>Something went wrong loading your account.</ErrorMessage>
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col gap-4 p-4">
      <TextInput
        label="Start Time"
        type="datetime-local"
        value={startTime}
        onChange={(event) => setStartTime(event.target.value)}
      />
      <TextInput
        label="Duration"
        type="number"
        min={15}
        max={120}
        value={duration}
        onChange={(event) => setDuration(event.target.value)}
        error={durationError}
      />
      <TextInput
        label="Location"
        value={location}
        onChange={(event) => setLocation(event.target.value)}
        error={locationError}
      />
      <TextInput
        label="Players Per Team"
        type="number"
        min={3}
        max={11}
        value={teamSize}
        onChange={(event) => setTeamSize(event.target.value)}
        error={teamSizeError}
      />
    </div>
  )
}
