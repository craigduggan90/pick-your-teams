import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router'
import { XIcon } from 'lucide-react'
import { Button } from '@/components/Button'
import { TextInput } from '@/components/TextInput'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'
import { toast } from '@/components/Toast'
import { useGame } from '@/hooks/useGame'
import { useSelf } from '@/hooks/useSelf'
import { useCreateInvitations } from '@/hooks/useCreateInvitations'
import { usePageTitle } from '@/hooks/usePageTitle'
import { usePageFooterActions } from '@/hooks/usePageActions'
import { ApiError } from '@/api/client'

let nextRowId = 0
function newRow() {
  return { id: nextRowId++, value: '' }
}

// Route /games/:id/invite, reached only from the Teams screen's "Invite Players" button
// (organiser-only, see EditTeamsView). 05-invite-players.png is stale — the mixed tag-or-email
// design with per-row claim tracking it shows was deleted from the API entirely. There's no
// current diagram for this screen; built directly against claude.md's tag-only contract instead:
// POST /invitations { GameId, UserTags[] }, all-or-nothing, errors rendered as a flat list with
// no per-row mapping back to a specific bad tag.
export function InvitePlayersPage() {
  usePageTitle('Invite Players')
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const gameQuery = useGame(id)
  const selfQuery = useSelf()
  const createInvitations = useCreateInvitations(id ?? '')

  const [rows, setRows] = useState(() => [newRow()])

  const game = gameQuery.data
  const isOrganiser = Boolean(
    game?.organiser && selfQuery.data && game.organiser.id === selfQuery.data.id,
  )

  // No non-organiser use case exists for this screen at all (unlike GameViewPage, which still has
  // a read-only mode) — bounce back to Teams rather than rendering a form with nothing to do.
  useEffect(() => {
    if (game && !isOrganiser) {
      navigate(`/games/${id}/teams`, { replace: true })
    }
  }, [game, isOrganiser, id, navigate])

  useEffect(() => {
    if (createInvitations.isSuccess) {
      toast.success('Invitations sent!')
      navigate(`/games/${id}/teams`, { replace: true })
    }
  }, [createInvitations.isSuccess, id, navigate])

  // CreateInvitations' 422s all carry a field-level `errors` dict (FluentValidation), even the
  // "Tag not found" ones (empty-string property name) — flattening every value regardless of key
  // is deliberate, per claude.md: "no need to match errors back to specific input rows by index."
  const fieldErrors =
    createInvitations.error instanceof ApiError
      ? Object.values(createInvitations.error.problem.errors ?? {}).flat()
      : []

  useEffect(() => {
    if (createInvitations.isError && fieldErrors.length === 0) {
      toast.error(
        createInvitations.error instanceof ApiError
          ? (createInvitations.error.problem.detail ?? createInvitations.error.message)
          : 'Something went wrong sending invitations.',
      )
    }
    // fieldErrors is derived from createInvitations.error and would just duplicate that
    // dependency; only toast a generic message for errors with nothing to show inline (e.g. a
    // 403/404 with no `errors` dict at all).
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [createInvitations.isError, createInvitations.error])

  function updateRow(rowId: number, value: string) {
    setRows((prev) => prev.map((row) => (row.id === rowId ? { ...row, value } : row)))
  }

  function removeRow(rowId: number) {
    setRows((prev) => prev.filter((row) => row.id !== rowId))
  }

  const tags = rows.map((row) => row.value.trim()).filter((value) => value.length > 0)
  const canSubmit = tags.length > 0

  usePageFooterActions(
    <div className="flex w-full justify-between gap-2 p-4">
      <Button variant="outline" onClick={() => navigate(`/games/${id}/teams`)}>
        Cancel
      </Button>
      <Button
        variant="primary"
        disabled={!canSubmit || createInvitations.isPending}
        onClick={() => createInvitations.mutate(tags)}
      >
        {createInvitations.isPending ? 'Sending…' : 'Send Invitations'}
      </Button>
    </div>,
  )

  if (gameQuery.isPending || selfQuery.isPending) {
    return <Loading />
  }

  if (gameQuery.isError || !game) {
    return <ErrorMessage>Something went wrong loading this game.</ErrorMessage>
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col gap-4 p-4">
      <p className="text-sm text-light-grey">
        Invite players by tag — they need an existing account to receive an invitation.
      </p>

      <div className="flex flex-col gap-3">
        {rows.map((row) => (
          <div key={row.id} className="flex items-center gap-2">
            <div className="flex-1">
              <TextInput label="Tag" value={row.value} onChange={(event) => updateRow(row.id, event.target.value)} />
            </div>
            {rows.length > 1 && (
              <Button
                variant="outline"
                className="size-12 shrink-0 px-0"
                aria-label="Remove tag"
                onClick={() => removeRow(row.id)}
              >
                <XIcon className="size-4" />
              </Button>
            )}
          </div>
        ))}
      </div>

      <Button variant="outline" onClick={() => setRows((prev) => [...prev, newRow()])}>
        + Add Another Tag
      </Button>

      {fieldErrors.length > 0 && (
        <div className="flex flex-col gap-1 rounded-lg border border-error/50 bg-error/10 p-3">
          {fieldErrors.map((message) => (
            <p key={message} className="text-sm text-error">
              {message}
            </p>
          ))}
        </div>
      )}
    </div>
  )
}
