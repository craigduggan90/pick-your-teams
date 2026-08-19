import type { ReactNode } from 'react'
import { RequireAuth } from './RequireAuth'
import { TagGate } from './TagGate'

/** RequireAuth, plus redirects to /tag-setup if the user hasn't set a tag yet. */
export function RequireAuthAndTag({ children }: { children: ReactNode }) {
  return (
    <RequireAuth>
      <TagGate>{children}</TagGate>
    </RequireAuth>
  )
}
