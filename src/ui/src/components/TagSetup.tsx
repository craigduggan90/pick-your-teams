import { useEffect, useState } from 'react'
import { TextInput } from '@/components/TextInput'
import { Button } from '@/components/Button'
import { useUpdateTag } from '@/hooks/useUpdateTag'
import { ApiError } from '@/api/client'
import { toast } from '@/components/Toast'

const TAG_REQUIREMENTS = [
  '3–36 characters',
  'Starts with a letter, number, or underscore',
  'Only letters, numbers, ".", "_", and "-" after that',
]

export type TagSetupMode = 'gate' | 'normal'

export interface TagSetupProps {
  mode: TagSetupMode
  userId: string | undefined
  currentTag?: string
  onSuccess?: () => void
  onCancel?: () => void
}

export function TagSetup({ mode, userId, currentTag, onSuccess, onCancel }: TagSetupProps) {
  const [tag, setTag] = useState(currentTag ?? '')
  const mutation = useUpdateTag(userId)

  useEffect(() => {
    if (mutation.isSuccess) {
      toast.success('Tag saved!')
      onSuccess?.()
    }
  }, [mutation.isSuccess, onSuccess])

  useEffect(() => {
    if (mutation.isError) {
      const message =
        mutation.error instanceof ApiError
          ? (mutation.error.problem.detail ?? mutation.error.message)
          : 'Something went wrong saving your tag.'
      toast.error(message)
    }
  }, [mutation.isError, mutation.error])

  const fieldError =
    mutation.error instanceof ApiError ? mutation.error.problem.errors?.Tag?.[0] : undefined

  const disabled = mutation.isPending || mutation.isSuccess

  return (
    <div className="mx-auto flex w-full max-w-md flex-col">
      <div className="flex flex-col gap-4 p-4">
        <p className="text-sm text-dark-grey">
          Your tag is the main way your friends will find you and invite you to games. Don't
          worry if you can't think of the perfect tag today, you can always change it later (as
          long as the tag you want is available!)
        </p>

        <TextInput
          label="Tag"
          value={tag}
          onChange={(event) => setTag(event.target.value)}
          error={fieldError}
          disabled={disabled}
        />

        <div>
          <p className="text-sm font-medium text-dark-grey">Requirements</p>
          <ul className="list-disc pl-5 text-sm text-light-grey">
            {TAG_REQUIREMENTS.map((requirement) => (
              <li key={requirement}>{requirement}</li>
            ))}
          </ul>
        </div>
      </div>

      <div className="mt-auto flex justify-end gap-2 border-t border-border p-4">
        {mode === 'normal' && (
          <Button variant="outline" onClick={onCancel} disabled={disabled}>
            Back
          </Button>
        )}
        <Button
          variant="primary"
          onClick={() => mutation.mutate(tag)}
          disabled={disabled || tag.length === 0}
        >
          {mutation.isPending ? 'Saving…' : 'Save'}
        </Button>
      </div>
    </div>
  )
}
