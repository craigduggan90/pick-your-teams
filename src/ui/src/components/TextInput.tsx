import { useEffect, useId, useState, type ComponentProps } from 'react'
import { Input } from '@/components/ui/input'
import { cn } from '@/lib/utils'

export interface TextInputProps extends Omit<ComponentProps<typeof Input>, 'id'> {
  label: string
  error?: string
  id?: string
}

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

  const floated = focused || hasValue
  const errorId = error ? `${inputId}-error` : undefined

  return (
    <div className="relative pt-3">
      <Input
        id={inputId}
        aria-invalid={Boolean(error) || undefined}
        aria-describedby={errorId}
        className={cn(
          'pt-3.5 pb-1.5',
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
            ? 'top-1.5 -translate-y-0 scale-75 text-primary'
            : 'top-1/2 -translate-y-1/2 scale-100',
        )}
      >
        {label}
      </label>
      {error && (
        <p id={errorId} className="mt-1 text-sm text-error">
          {error}
        </p>
      )}
    </div>
  )
}
