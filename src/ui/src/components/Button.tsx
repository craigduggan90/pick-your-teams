import type { ComponentProps } from 'react'
import type { VariantProps } from 'class-variance-authority'
import { Button as BaseButton, buttonVariants } from '@/components/ui/button'
import { cn } from '@/lib/utils'

type BaseVariant = NonNullable<VariantProps<typeof buttonVariants>['variant']>

export type ButtonVariant =
  | 'primary'
  | 'secondary'
  | 'destructive'
  | 'outline'
  | 'ghost'
  | 'link'

const VARIANT_MAP: Record<ButtonVariant, BaseVariant> = {
  primary: 'default',
  secondary: 'secondary',
  destructive: 'destructive',
  outline: 'outline',
  ghost: 'ghost',
  link: 'link',
}

export interface ButtonProps extends Omit<ComponentProps<typeof BaseButton>, 'variant'> {
  variant?: ButtonVariant
}

export function Button({ variant = 'primary', className, ...props }: ButtonProps) {
  return (
    <BaseButton
      variant={VARIANT_MAP[variant]}
      className={cn('h-12 px-4', className)}
      {...props}
    />
  )
}
