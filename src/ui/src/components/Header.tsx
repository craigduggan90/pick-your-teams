import type { ReactNode } from 'react'

export interface HeaderProps {
  title: string
  /** Right-side account/avatar slot. Defaults to an unwired placeholder until routing exists. */
  accountSlot?: ReactNode
}

export function Header({ title, accountSlot }: HeaderProps) {
  return (
    <header className="flex items-center justify-between bg-primary px-4 py-3 text-primary-foreground">
      <h1 className="truncate text-lg font-medium">{title}</h1>
      {accountSlot ?? (
        <span
          aria-hidden="true"
          className="size-8 shrink-0 rounded-full bg-primary-foreground/30"
        />
      )}
    </header>
  )
}
