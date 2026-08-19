import { useAuth0 } from '@auth0/auth0-react'
import { Button } from '@/components/Button'
import { TagGate } from '@/components/TagGate'
import { Loading } from '@/components/Loading'
import { usePageTitle } from '@/hooks/usePageTitle'
import { APP_NAME } from '@/lib/constants'

function HomePlaceholder() {
  return <p className="p-4 text-sm text-light-grey">Screens land in later stages.</p>
}

export function TeamPickerPage() {
  usePageTitle(APP_NAME)
  const { isAuthenticated, isLoading, loginWithRedirect } = useAuth0()

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
