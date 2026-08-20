import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router'
import { useAuth0 } from '@auth0/auth0-react'
import { PageTitleProvider, useHeaderTitle } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { useSelf } from '@/hooks/useSelf'
import { useUpdateProfile } from '@/hooks/useUpdateProfile'
import { useDeleteAccount } from '@/hooks/useDeleteAccount'
import { ApiError } from '@/api/client'
import type { ChangeTagLocationState } from '@/lib/navigation'
import { MyAccountPage } from './MyAccountPage'

vi.mock('@auth0/auth0-react')
vi.mock('@/hooks/useSelf')
vi.mock('@/hooks/useUpdateProfile')
vi.mock('@/hooks/useDeleteAccount')

const self = {
  id: 'user-1',
  tag: 'little-bobby-tables',
  displayName: 'Robert D. Tables',
  rating: 1200,
  email: 'bob@example.com',
  mobile: null,
  created: '2026-01-01T00:00:00.000Z',
  modified: '2026-01-01T00:00:00.000Z',
}

function HeaderTitleStub() {
  return <h1>{useHeaderTitle()}</h1>
}

function FooterActionsStub() {
  return <>{useFooterActions()}</>
}

function ChangeTagStub() {
  const location = useLocation()
  const state = location.state as ChangeTagLocationState | null
  return <p>Change tag screen, from: {state?.from ?? '(none)'}</p>
}

function renderPage() {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <PageActionsProvider>
        <HeaderTitleStub />
        <MemoryRouter initialEntries={['/account']}>
          <Routes>
            <Route path="/account" element={<MyAccountPage />} />
            <Route path="/change-tag" element={<ChangeTagStub />} />
            <Route path="/" element={<p>Games list</p>} />
          </Routes>
        </MemoryRouter>
        <FooterActionsStub />
      </PageActionsProvider>
    </PageTitleProvider>,
  )
}

function mockMutations(overrides: { update?: any; del?: any } = {}) {
  const updateMutate = vi.fn()
  const deleteMutate = vi.fn()
  vi.mocked(useUpdateProfile).mockReturnValue({
    mutate: updateMutate,
    isPending: false,
    isSuccess: false,
    isError: false,
    error: null,
    ...overrides.update,
  } as any)
  vi.mocked(useDeleteAccount).mockReturnValue({
    mutate: deleteMutate,
    isPending: false,
    isSuccess: false,
    ...overrides.del,
  } as any)
  return { updateMutate, deleteMutate }
}

describe('MyAccountPage', () => {
  it('shows a loading state', () => {
    vi.mocked(useAuth0).mockReturnValue({ logout: vi.fn() } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: true, isError: false } as any)
    mockMutations()

    renderPage()

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error state', () => {
    vi.mocked(useAuth0).mockReturnValue({ logout: vi.fn() } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: true, data: undefined } as any)
    mockMutations()

    renderPage()

    expect(screen.getByText('Something went wrong loading your account.')).toBeInTheDocument()
  })

  it('sets the header title to the user\'s tag and shows the profile fields prefilled', () => {
    vi.mocked(useAuth0).mockReturnValue({ logout: vi.fn() } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: self } as any)
    mockMutations()

    renderPage()

    expect(screen.getByRole('heading')).toHaveTextContent('@little-bobby-tables')
    expect(screen.getByLabelText('Display Name')).toHaveValue('Robert D. Tables')
    expect(screen.getByLabelText('Email Address')).toHaveValue('bob@example.com')
  })

  it('shows a placeholder header title while the user is still loading', () => {
    vi.mocked(useAuth0).mockReturnValue({ logout: vi.fn() } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: true, isError: false, data: undefined } as any)
    mockMutations()

    renderPage()

    expect(screen.getByRole('heading')).toHaveTextContent('My Account')
  })

  it('saves the display name and email via the footer Save button', async () => {
    vi.mocked(useAuth0).mockReturnValue({ logout: vi.fn() } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: self } as any)
    const { updateMutate } = mockMutations()
    const user = userEvent.setup()

    renderPage()
    await user.clear(screen.getByLabelText('Display Name'))
    await user.type(screen.getByLabelText('Display Name'), 'New Name')
    await user.click(screen.getByRole('button', { name: 'Save' }))

    expect(updateMutate).toHaveBeenCalledWith({ DisplayName: 'New Name', Email: 'bob@example.com' })
  })

  it('shows field errors from a validation failure', () => {
    vi.mocked(useAuth0).mockReturnValue({ logout: vi.fn() } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: self } as any)
    const error = new ApiError(422, {
      title: 'Validation Error',
      errors: { Email: ['Email must be a valid email address.'] },
    })
    mockMutations({ update: { isError: true, error } })

    renderPage()

    expect(screen.getByText('Email must be a valid email address.')).toBeInTheDocument()
  })

  it('navigates to /change-tag with the current path as the return state', async () => {
    vi.mocked(useAuth0).mockReturnValue({ logout: vi.fn() } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: self } as any)
    mockMutations()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Change Tag' }))

    expect(screen.getByText('Change tag screen, from: /account')).toBeInTheDocument()
  })

  it('logs out via Auth0, returning to the home page with a logged_out marker', async () => {
    const logout = vi.fn()
    vi.mocked(useAuth0).mockReturnValue({ logout } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: self } as any)
    mockMutations()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Log Out' }))

    expect(logout).toHaveBeenCalledWith({
      logoutParams: { returnTo: `${window.location.origin}/?logged_out=true` },
    })
  })

  it('opens the delete confirmation and deletes on confirm', async () => {
    vi.mocked(useAuth0).mockReturnValue({ logout: vi.fn() } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: self } as any)
    const { deleteMutate } = mockMutations()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Delete Account' }))
    expect(screen.getByText('This cannot be undone.')).toBeInTheDocument()

    // The Dialog primitive marks background content aria-hidden while open, so only the modal's
    // own button is queryable here, not a second element to index into.
    await user.click(screen.getByRole('button', { name: 'Delete Account' }))

    expect(deleteMutate).toHaveBeenCalled()
  })

  it('logs out (redirecting to login) once account deletion succeeds', () => {
    const logout = vi.fn()
    vi.mocked(useAuth0).mockReturnValue({ logout } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: self } as any)
    mockMutations({ del: { isSuccess: true } })

    renderPage()

    expect(logout).toHaveBeenCalledWith({
      logoutParams: { returnTo: `${window.location.origin}/?logged_out=true` },
    })
  })

  it('navigates home via the footer Back button', async () => {
    vi.mocked(useAuth0).mockReturnValue({ logout: vi.fn() } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: self } as any)
    mockMutations()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Back' }))

    expect(screen.getByText('Games list')).toBeInTheDocument()
  })
})
