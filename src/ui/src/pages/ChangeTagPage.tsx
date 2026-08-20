import { useLocation, useNavigate } from 'react-router'
import { useSelf } from '@/hooks/useSelf'
import { usePageTitle } from '@/hooks/usePageTitle'
import { ChangeTag } from '@/components/ChangeTag'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'
import type { ChangeTagLocationState } from '@/lib/navigation'

export function ChangeTagPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const selfQuery = useSelf()

  const needsTag = selfQuery.data ? selfQuery.data.id === selfQuery.data.tag : undefined
  usePageTitle(needsTag === false ? 'Change Tag' : 'Set Your Tag')

  if (selfQuery.isPending) {
    return <Loading />
  }

  if (selfQuery.isError) {
    return <ErrorMessage>Something went wrong loading your account.</ErrorMessage>
  }

  const returnTo = (location.state as ChangeTagLocationState | null)?.from ?? '/'
  const goBack = () => navigate(returnTo, { replace: true })

  return (
    <ChangeTag
      mode={needsTag ? 'gate' : 'normal'}
      userId={selfQuery.data.id}
      currentTag={needsTag ? undefined : selfQuery.data.tag}
      onSuccess={goBack}
      onCancel={goBack}
    />
  )
}
