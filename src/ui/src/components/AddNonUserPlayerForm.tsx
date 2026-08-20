import { useState } from 'react'
import { Button } from '@/components/Button'
import { TextInput } from '@/components/TextInput'

export interface AddNonUserPlayerFormProps {
  onSubmit: (displayName: string, estimatedRating: number) => void
  isPending: boolean
  displayNameError?: string
  ratingError?: string
  /** Collapsed-trigger only — the roster's already at capacity. */
  disabled?: boolean
}

// Inline collapsible section per 06-b-view-teams.png, not a modal. The parent remounts this
// (via a changing `key`) after a successful add, which is what collapses it back and clears the
// fields — see EditTeamsView. Sized to sit side-by-side with the Invite Players button when
// collapsed (`flex-1`, in a `flex flex-wrap` row); `w-full` on the expanded form forces it onto
// its own line in that same wrapping row instead of squeezing next to Invite Players.
export function AddNonUserPlayerForm({
  onSubmit,
  isPending,
  displayNameError,
  ratingError,
  disabled = false,
}: AddNonUserPlayerFormProps) {
  const [open, setOpen] = useState(false)
  const [displayName, setDisplayName] = useState('')
  const [rating, setRating] = useState('1000')

  if (!open) {
    return (
      <Button variant="outline" className="flex-1" disabled={disabled} onClick={() => setOpen(true)}>
        Add Non-User Player
      </Button>
    )
  }

  return (
    <div className="flex w-full flex-col gap-3 rounded-lg border border-border p-3">
      <TextInput
        label="Display Name"
        value={displayName}
        onChange={(event) => setDisplayName(event.target.value)}
        error={displayNameError}
      />
      <TextInput
        label="Rating"
        type="number"
        min={1}
        max={2000}
        value={rating}
        onChange={(event) => setRating(event.target.value)}
        error={ratingError}
      />
      <div className="flex justify-between gap-2">
        <Button variant="outline" onClick={() => setOpen(false)} disabled={isPending}>
          Cancel
        </Button>
        <Button
          variant="primary"
          disabled={isPending || !displayName.trim()}
          onClick={() => onSubmit(displayName.trim(), Number(rating))}
        >
          {isPending ? 'Adding…' : 'Add'}
        </Button>
      </div>
    </div>
  )
}
