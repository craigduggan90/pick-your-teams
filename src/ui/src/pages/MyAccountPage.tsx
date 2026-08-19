import { useLocation, useNavigate } from 'react-router'
import { Button } from '@/components/Button'
import { usePageTitle } from '@/hooks/usePageTitle'
import type { ChangeTagLocationState } from '@/lib/navigation'

export function MyAccountPage() {
  usePageTitle('My Account')
  const navigate = useNavigate()
  const location = useLocation()

  const goToChangeTag = () => {
    const state: ChangeTagLocationState = { from: location.pathname }
    navigate('/change-tag', { state })
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
    </div>
  )
}
