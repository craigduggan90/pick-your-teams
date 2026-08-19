import { useAuth0 } from '@auth0/auth0-react'
import { useLocation, useNavigate } from 'react-router'
import { Button } from '@/components/Button'
import { usePageTitle } from '@/hooks/usePageTitle'
import type { ChangeTagLocationState } from '@/lib/navigation'
import { LOGGED_OUT_QUERY_PARAM } from '@/lib/constants'

export function MyAccountPage() {
  usePageTitle('My Account')
  const navigate = useNavigate()
  const location = useLocation()
  const { logout } = useAuth0()

  const goToChangeTag = () => {
    const state: ChangeTagLocationState = { from: location.pathname }
    navigate('/change-tag', { state })
  }

  const handleLogOut = () => {
    logout({
      logoutParams: { returnTo: `${window.location.origin}/?${LOGGED_OUT_QUERY_PARAM}=true` },
    })
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-col gap-4 p-4">
      <p className="text-sm text-light-grey">
        The full My Account screen lands in Stage 3 — this is a placeholder so Change Tag has
        somewhere to link from for now.
      </p>
      <Button variant="outline" className="w-full" onClick={goToChangeTag}>
        Change Tag
      </Button>
      <Button variant="outline" className="w-full" onClick={handleLogOut}>
        Log Out
      </Button>
    </div>
  )
}
