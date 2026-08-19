import { useNavigate } from 'react-router'
import { useSelf } from '@/hooks/useSelf'
import { usePageTitle } from '@/hooks/usePageTitle'
import { TagSetup } from '@/components/TagSetup'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'

export function TagSetupPage() {
  usePageTitle('Set Your Tag')
  const navigate = useNavigate()
  const selfQuery = useSelf()

  if (selfQuery.isPending) {
    return <Loading />
  }

  if (selfQuery.isError) {
    return <ErrorMessage>Something went wrong loading your account.</ErrorMessage>
  }

  return <TagSetup mode="gate" userId={selfQuery.data.id} onSuccess={() => navigate('/')} />
}
