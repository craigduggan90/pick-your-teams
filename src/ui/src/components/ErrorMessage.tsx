import type { ReactNode } from 'react'

export function ErrorMessage({ children }: { children: ReactNode }) {
  return <p className="p-4 text-center text-sm text-error">{children}</p>
}
