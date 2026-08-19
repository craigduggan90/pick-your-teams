import type { ReactNode } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { Navigate } from 'react-router'
import { Loading } from './Loading'

/** Redirects to the public landing page ("/") if not authenticated. No auto Auth0 redirect. */
export function RequireAuth({ children }: { children: ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth0()

  if (isLoading) {
    return <Loading />
  }

  if (!isAuthenticated) {
    return <Navigate to="/" replace />
  }

  return <>{children}</>
}
