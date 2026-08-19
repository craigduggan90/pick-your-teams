import type { ReactNode } from 'react'
import { Navigate } from 'react-router'
import { useSelf } from '@/hooks/useSelf'
import { Loading } from './Loading'
import { ErrorMessage } from './ErrorMessage'

/**
 * Renders children once the current user is confirmed to have a tag set; redirects to
 * /tag-setup otherwise. Assumes the caller already knows the user is authenticated.
 */
export function TagGate({ children }: { children: ReactNode }) {
  const selfQuery = useSelf()

  if (selfQuery.isPending) {
    return <Loading />
  }

  if (selfQuery.isError) {
    return <ErrorMessage>Something went wrong loading your account.</ErrorMessage>
  }

  if (selfQuery.data.Id === selfQuery.data.Tag) {
    return <Navigate to="/tag-setup" replace />
  }

  return <>{children}</>
}
