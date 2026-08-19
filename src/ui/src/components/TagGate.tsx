import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router'
import { useSelf } from '@/hooks/useSelf'
import type { ChangeTagLocationState } from '@/lib/navigation'
import { Loading } from './Loading'
import { ErrorMessage } from './ErrorMessage'

/**
 * Renders children once the current user is confirmed to have a tag set; redirects to
 * /change-tag otherwise. Assumes the caller already knows the user is authenticated.
 */
export function TagGate({ children }: { children: ReactNode }) {
  const location = useLocation()
  const selfQuery = useSelf()

  if (selfQuery.isPending) {
    return <Loading />
  }

  if (selfQuery.isError) {
    return <ErrorMessage>Something went wrong loading your account.</ErrorMessage>
  }

  if (selfQuery.data.id === selfQuery.data.tag) {
    const state: ChangeTagLocationState = { from: location.pathname }
    return <Navigate to="/change-tag" state={state} replace />
  }

  return <>{children}</>
}
