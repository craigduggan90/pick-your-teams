import { useEffect, useRef } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { useSearchParams } from 'react-router'
import { Button } from '@/components/Button'
import { TagGate } from '@/components/TagGate'
import { Loading } from '@/components/Loading'
import { toast } from '@/components/Toast'
import { usePageTitle } from '@/hooks/usePageTitle'
import { APP_NAME, LOGGED_OUT_QUERY_PARAM } from '@/lib/constants'

function HomePlaceholder() {
  return <p className="p-4 text-sm text-light-grey">Screens land in later stages.</p>
}

export function TeamPickerPage() {
  usePageTitle(APP_NAME)
  const { isAuthenticated, isLoading, loginWithRedirect } = useAuth0()
  const [searchParams, setSearchParams] = useSearchParams()
  const hasShownLoggedOutToast = useRef(false)

  useEffect(() => {
    if (!searchParams.has(LOGGED_OUT_QUERY_PARAM) || hasShownLoggedOutToast.current) {
      return
    }
    // Guards against StrictMode's dev-only double effect invocation, which would otherwise fire
    // this twice before the setSearchParams below has re-rendered with the param removed.
    hasShownLoggedOutToast.current = true
    toast.success("You've been logged out.")
    setSearchParams(
      (params) => {
        params.delete(LOGGED_OUT_QUERY_PARAM)
        return params
      },
      { replace: true },
    )
  }, [searchParams, setSearchParams])

  if (isLoading) {
    return <Loading />
  }

  if (isAuthenticated) {
    return (
      <TagGate>
        <HomePlaceholder />
      </TagGate>
    )
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-col items-center gap-6 p-8 text-center">
      <div className="flex h-64 w-full items-center justify-center rounded-lg border border-dashed border-border text-sm text-light-grey">
        Team Picker
      </div>
      <div className="flex w-full flex-col gap-3">
        <Button variant="primary" className="w-full" onClick={() => loginWithRedirect()}>
          Log In
        </Button>
        <Button
          variant="outline"
          className="w-full"
          onClick={() => loginWithRedirect({ authorizationParams: { screen_hint: 'signup' } })}
        >
          Register
        </Button>
      </div>
    </div>
  )
}
