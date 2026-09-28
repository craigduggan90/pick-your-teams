import { useEffect, useState } from 'react'
import { Sheet } from '@/components/Sheet'
import { Button } from '@/components/Button'
import { TextInput } from '@/components/TextInput'
import { oneWeekAfter } from '@/lib/format'

export interface CopyGameModalProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  sourceStartTime: string
  onConfirm: (startTime: string) => void
  isPending: boolean
}

export function CopyGameModal({
  open,
  onOpenChange,
  sourceStartTime,
  onConfirm,
  isPending,
}: CopyGameModalProps) {
  const [startTime, setStartTime] = useState('')

  useEffect(() => {
    if (open) {
      setStartTime(oneWeekAfter(sourceStartTime))
    }
  }, [open, sourceStartTime])

  return (
    <Sheet
      open={open}
      onOpenChange={onOpenChange}
      title="Copy Game"
      description="Everything else — location, duration, players and their current ratings — carries over. Players will be unassigned; you can generate or set teams again once the date is set."
      footer={
        <>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={isPending}>
            Cancel
          </Button>
          <Button
            variant="primary"
            onClick={() => startTime && onConfirm(startTime)}
            disabled={!startTime || isPending}
          >
            {isPending ? 'Copying…' : 'Copy Game'}
          </Button>
        </>
      }
    >
      <TextInput
        label="Start Time"
        type="datetime-local"
        value={startTime}
        onChange={(event) => setStartTime(event.target.value)}
      />
    </Sheet>
  )
}
