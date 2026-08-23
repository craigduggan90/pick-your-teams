import { useEffect, useId, useState, type ComponentProps } from 'react'
import { Input } from '@/components/ui/input'
import { cn } from '@/lib/utils'

export interface TextInputProps extends Omit<ComponentProps<typeof Input>, 'id'> {
  label: string
  error?: string
  id?: string
}

// Native date/time picker types always show their own placeholder-shaped content
// ("dd/mm/yyyy, --:--") even when empty, and their internal rendering (Safari's especially)
// isn't guaranteed to respect the padding-top trick the floating label overlaps into. Forcing
// these to always render floated sidesteps both problems: the
// label never sits in the larger unfloated position that was colliding with the native control,
// and every field still shares the same label styling instead of a different static layout.
const NATIVE_PICKER_TYPES = new Set(['date', 'time', 'datetime-local', 'month', 'week'])

export function TextInput({
  label,
  error,
  className,
  onFocus,
  onBlur,
  onChange,
  value,
  defaultValue,
  id,
  type,
  ...props
}: TextInputProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const [focused, setFocused] = useState(false)
  const [hasValue, setHasValue] = useState(() => Boolean(value ?? defaultValue))

  useEffect(() => {
    if (value !== undefined) {
      setHasValue(String(value).length > 0)
    }
  }, [value])

  const isNativePicker = Boolean(type && NATIVE_PICKER_TYPES.has(type))
  const floated = isNativePicker || focused || hasValue
  const errorId = error ? `${inputId}-error` : undefined

  return (
    <div>
      <div className="relative">
        <Input
          id={inputId}
          type={type}
          aria-invalid={Boolean(error) || undefined}
          aria-describedby={errorId}
          className={cn(
            'h-12 pt-5 pb-1 text-base',
            error && 'border-error focus-visible:ring-error/50',
            className,
          )}
          value={value}
          defaultValue={defaultValue}
          onFocus={(event) => {
            setFocused(true)
            onFocus?.(event)
          }}
          onBlur={(event) => {
            setFocused(false)
            onBlur?.(event)
          }}
          onChange={(event) => {
            setHasValue(event.currentTarget.value.length > 0)
            onChange?.(event)
          }}
          {...props}
        />
        <label
          htmlFor={inputId}
          data-floated={floated || undefined}
          className={cn(
            'pointer-events-none absolute left-2.5 origin-left text-base text-muted-foreground transition-all',
            floated
              ? 'top-0.5 -translate-y-0 scale-75 text-primary'
              : 'top-1/2 -translate-y-1/2 scale-100',
          )}
        >
          {label}
        </label>
      </div>
      {error && (
        <p id={errorId} className="mt-1 text-sm text-error">
          {error}
        </p>
      )}
    </div>
  )
}
