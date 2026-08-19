import { useId } from 'react'
import {
  Select as BaseSelect,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { cn } from '@/lib/utils'

export interface SelectOption<T extends string = string> {
  value: T
  label: string
  /** Styled as a destructive action, e.g. "Remove from Game". */
  destructive?: boolean
}

export interface SelectFieldProps<T extends string = string> {
  label?: string
  placeholder?: string
  value?: T
  onValueChange?: (value: T) => void
  options: SelectOption<T>[]
  disabled?: boolean
  className?: string
}

export function SelectField<T extends string = string>({
  label,
  placeholder = 'Select…',
  value,
  onValueChange,
  options,
  disabled,
  className,
}: SelectFieldProps<T>) {
  const id = useId()

  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={id} className="text-sm text-dark-grey">
          {label}
        </label>
      )}
      <BaseSelect
        items={options.map((option) => ({ value: option.value, label: option.label }))}
        value={value}
        onValueChange={(next) => onValueChange?.(next as T)}
        disabled={disabled}
      >
        <SelectTrigger id={id} className={cn('w-full data-[size=default]:h-12', className)}>
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>
        <SelectContent>
          {options.map((option) => (
            <SelectItem
              key={option.value}
              value={option.value}
              className={cn(option.destructive && 'text-error focus:text-error')}
            >
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </BaseSelect>
    </div>
  )
}
