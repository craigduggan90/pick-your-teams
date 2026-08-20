import { useEffect, useState } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { useLocation, useNavigate } from 'react-router'
import { Button } from '@/components/Button'
import { TextInput } from '@/components/TextInput'
import { Modal } from '@/components/Modal'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'
import { toast } from '@/components/Toast'
import { useSelf } from '@/hooks/useSelf'
import { useUpdateProfile } from '@/hooks/useUpdateProfile'
import { useDeleteAccount } from '@/hooks/useDeleteAccount'
import { usePageTitle } from '@/hooks/usePageTitle'
import { usePageFooterActions } from '@/hooks/usePageActions'
import { ApiError } from '@/api/client'
import type { ChangeTagLocationState } from '@/lib/navigation'
import { LOGGED_OUT_QUERY_PARAM } from '@/lib/constants'

export function MyAccountPage() {
  usePageTitle('My Account')
  const navigate = useNavigate()
  const location = useLocation()
  const { logout } = useAuth0()
  const selfQuery = useSelf()
  const updateProfile = useUpdateProfile(selfQuery.data?.id)
  const deleteAccount = useDeleteAccount(selfQuery.data?.id)

  const [displayName, setDisplayName] = useState('')
  const [email, setEmail] = useState('')
  const [deleteOpen, setDeleteOpen] = useState(false)

  const self = selfQuery.data

  useEffect(() => {
    if (self) {
      setDisplayName(self.displayName)
      setEmail(self.email)
    }
  }, [self])

  useEffect(() => {
    if (updateProfile.isSuccess) {
      toast.success('Changes Saved!')
    }
  }, [updateProfile.isSuccess])

  useEffect(() => {
    if (updateProfile.isError) {
      const message =
        updateProfile.error instanceof ApiError
          ? (updateProfile.error.problem.detail ?? updateProfile.error.message)
          : 'Something went wrong saving your account.'
      toast.error(message)
    }
  }, [updateProfile.isError, updateProfile.error])

  const returnToLoggedOut = () =>
    logout({
      logoutParams: { returnTo: `${window.location.origin}/?${LOGGED_OUT_QUERY_PARAM}=true` },
    })

  useEffect(() => {
    // Delete & redirect to login, per 03-my-account.png's modal annotation — reuses the same
    // Auth0 logout redirect as a voluntary Log Out, since the account (and its session) are gone
    // either way.
    if (deleteAccount.isSuccess) {
      returnToLoggedOut()
    }
    // Deliberately depends only on deleteAccount.isSuccess — returnToLoggedOut is a fresh
    // function every render and doesn't need to retrigger this effect.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [deleteAccount.isSuccess])

  const displayNameError =
    updateProfile.error instanceof ApiError
      ? updateProfile.error.problem.errors?.DisplayName?.[0]
      : undefined
  const emailError =
    updateProfile.error instanceof ApiError ? updateProfile.error.problem.errors?.Email?.[0] : undefined

  const goToChangeTag = () => {
    const state: ChangeTagLocationState = { from: location.pathname }
    navigate('/change-tag', { state })
  }

  usePageFooterActions(
    self && (
      <div className="flex w-full justify-between gap-2 p-4">
        <Button variant="outline" onClick={() => navigate('/')}>
          Back
        </Button>
        <Button
          variant="primary"
          disabled={updateProfile.isPending}
          onClick={() => updateProfile.mutate({ DisplayName: displayName, Email: email })}
        >
          {updateProfile.isPending ? 'Saving…' : 'Save'}
        </Button>
      </div>
    ),
  )

  if (selfQuery.isPending) {
    return <Loading />
  }

  if (selfQuery.isError || !self) {
    return <ErrorMessage>Something went wrong loading your account.</ErrorMessage>
  }

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col gap-4 p-4">
      <p className="text-sm text-light-grey">@{self.tag}</p>

      <TextInput
        label="Display Name"
        value={displayName}
        onChange={(event) => setDisplayName(event.target.value)}
        error={displayNameError}
      />
      <TextInput
        label="Email Address"
        type="email"
        value={email}
        onChange={(event) => setEmail(event.target.value)}
        error={emailError}
      />

      <Button variant="outline" onClick={goToChangeTag}>
        Change Tag
      </Button>
      <Button variant="destructive" onClick={() => setDeleteOpen(true)}>
        Delete Account
      </Button>
      <Button variant="outline" onClick={returnToLoggedOut}>
        Log Out
      </Button>

      <Modal
        open={deleteOpen}
        onOpenChange={setDeleteOpen}
        title="Delete Account?"
        description="This cannot be undone."
        footer={
          <>
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteAccount.isPending}
              onClick={() => deleteAccount.mutate()}
            >
              {deleteAccount.isPending ? 'Deleting…' : 'Delete Account'}
            </Button>
          </>
        }
      />
    </div>
  )
}
