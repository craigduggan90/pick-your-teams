import { useEffect, useState } from 'react'
import { Modal } from '@/components/Modal'
import { Button } from '@/components/Button'
import { cn } from '@/lib/utils'
import type { GameWinner } from '@/api/games'

const OPTIONS: { value: GameWinner; label: string }[] = [
  { value: 'Home', label: 'Home Team' },
  { value: 'Away', label: 'Away Team' },
  // The API's own Swagger examples label GameTeamEnum.None as "Draw" for this endpoint — see
  // RecordResultRequestModelExample.NoWinnerExample.
  { value: 'None', label: 'Draw' },
]

export interface RecordResultModalProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onConfirm: (winner: GameWinner) => void
  isPending: boolean
}

export function RecordResultModal({ open, onOpenChange, onConfirm, isPending }: RecordResultModalProps) {
  const [winner, setWinner] = useState<GameWinner | undefined>(undefined)

  useEffect(() => {
    if (open) {
      setWinner(undefined)
    }
  }, [open])

  return (
    <Modal
      open={open}
      onOpenChange={onOpenChange}
      title="Record Result"
      footer={
        <>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={isPending}>
            Cancel
          </Button>
          <Button
            variant="primary"
            onClick={() => winner && onConfirm(winner)}
            disabled={!winner || isPending}
          >
            {isPending ? 'Saving…' : 'Confirm'}
          </Button>
        </>
      }
    >
      <div className="flex flex-col gap-2">
        {OPTIONS.map((option) => (
          <button
            key={option.value}
            type="button"
            aria-pressed={winner === option.value}
            onClick={() => setWinner(option.value)}
            disabled={isPending}
            className={cn(
              'cursor-pointer rounded-lg border px-3 py-2 text-left text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50',
              winner === option.value
                ? 'border-info bg-info/10 text-info'
                : 'border-border bg-background text-dark-grey hover:bg-muted',
            )}
          >
            {option.label}
          </button>
        ))}
      </div>
    </Modal>
  )
}
