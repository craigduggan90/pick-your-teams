import { StrictMode } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes, useSearchParams } from 'react-router'
import { useAuth0 } from '@auth0/auth0-react'
import { useSelf } from '@/hooks/useSelf'
import { useGames } from '@/hooks/useGames'
import { PageTitleProvider } from '@/hooks/usePageTitle'
import { PageActionsProvider } from '@/hooks/usePageActions'
import { toast } from '@/components/Toast'
import { TeamPickerPage } from './TeamPickerPage'

vi.mock('@auth0/auth0-react')
vi.mock('@/hooks/useSelf')
vi.mock('@/hooks/useGames')
vi.mock('@/components/Toast', () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}))

function SearchParamsProbe() {
  const [searchParams] = useSearchParams()
  return <p>search: {searchParams.toString() || '(empty)'}</p>
}

function renderPage(initialEntry = '/', { strict = false } = {}) {
  const tree = (
    <PageTitleProvider initialTitle="Pick Your Teams">
      <PageActionsProvider>
        <MemoryRouter initialEntries={[initialEntry]}>
          <SearchParamsProbe />
          <Routes>
            <Route path="/" element={<TeamPickerPage />} />
            <Route path="/change-tag" element={<p>Change tag screen</p>} />
          </Routes>
        </MemoryRouter>
      </PageActionsProvider>
    </PageTitleProvider>
  )
  return render(strict ? <StrictMode>{tree}</StrictMode> : tree)
}

describe('TeamPickerPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows Log In / Register when not authenticated', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      loginWithRedirect: vi.fn(),
    } as unknown as ReturnType<typeof useAuth0>)
    vi.mocked(useSelf).mockReturnValue({ isPending: true } as unknown as ReturnType<typeof useSelf>)

    renderPage()

    expect(screen.getByRole('button', { name: 'Log In' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Register' })).toBeInTheDocument()
  })

  it('calls loginWithRedirect with a signup hint for Register', async () => {
    const loginWithRedirect = vi.fn()
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      loginWithRedirect,
    } as unknown as ReturnType<typeof useAuth0>)
    vi.mocked(useSelf).mockReturnValue({ isPending: true } as unknown as ReturnType<typeof useSelf>)

    const user = userEvent.setup()
    renderPage()
    await user.click(screen.getByRole('button', { name: 'Register' }))

    expect(loginWithRedirect).toHaveBeenCalledWith({
      authorizationParams: { screen_hint: 'signup' },
    })
  })

  it('redirects to /change-tag when the user still needs to set a tag', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      loginWithRedirect: vi.fn(),
    } as unknown as ReturnType<typeof useAuth0>)
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: '1', tag: '1' },
    } as unknown as ReturnType<typeof useSelf>)

    renderPage()

    expect(screen.getByText('Change tag screen')).toBeInTheDocument()
  })

  it('renders the games list once tagged', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      loginWithRedirect: vi.fn(),
    } as unknown as ReturnType<typeof useAuth0>)
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: '1', tag: 'bob' },
    } as unknown as ReturnType<typeof useSelf>)
    vi.mocked(useGames).mockReturnValue({
      data: { pages: [{ data: [], cursor: null, count: 0 }] },
      isPending: false,
      isError: false,
      isSuccess: true,
      hasNextPage: false,
    } as unknown as ReturnType<typeof useGames>)

    renderPage()

    expect(screen.getByText('No Games Found!')).toBeInTheDocument()
  })

  it('shows a logged-out toast and strips the query param when ?logged_out=true', async () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      loginWithRedirect: vi.fn(),
    } as unknown as ReturnType<typeof useAuth0>)
    vi.mocked(useSelf).mockReturnValue({ isPending: true } as unknown as ReturnType<typeof useSelf>)

    renderPage('/?logged_out=true')

    await waitFor(() => expect(toast.success).toHaveBeenCalledWith("You've been logged out."))
    await waitFor(() => expect(screen.getByText('search: (empty)')).toBeInTheDocument())
  })

  it('shows the logged-out toast exactly once under StrictMode', async () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      loginWithRedirect: vi.fn(),
    } as unknown as ReturnType<typeof useAuth0>)
    vi.mocked(useSelf).mockReturnValue({ isPending: true } as unknown as ReturnType<typeof useSelf>)

    renderPage('/?logged_out=true', { strict: true })

    await waitFor(() => expect(toast.success).toHaveBeenCalled())
    expect(toast.success).toHaveBeenCalledTimes(1)
  })

  it('does not toast when there is no logged_out param', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      loginWithRedirect: vi.fn(),
    } as unknown as ReturnType<typeof useAuth0>)
    vi.mocked(useSelf).mockReturnValue({ isPending: true } as unknown as ReturnType<typeof useSelf>)

    renderPage('/')

    expect(toast.success).not.toHaveBeenCalled()
  })

  it('shows an error state if the self query fails', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      loginWithRedirect: vi.fn(),
    } as unknown as ReturnType<typeof useAuth0>)
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: true,
    } as unknown as ReturnType<typeof useSelf>)

    renderPage()

    expect(screen.getByText('Something went wrong loading your account.')).toBeInTheDocument()
  })
})
