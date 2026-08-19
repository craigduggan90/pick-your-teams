import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { useAuth0 } from '@auth0/auth0-react'
import { useSelf } from '@/hooks/useSelf'
import { PageTitleProvider } from '@/hooks/usePageTitle'
import { TeamPickerPage } from './TeamPickerPage'

vi.mock('@auth0/auth0-react')
vi.mock('@/hooks/useSelf')

function renderPage() {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <MemoryRouter initialEntries={['/']}>
        <Routes>
          <Route path="/" element={<TeamPickerPage />} />
          <Route path="/tag-setup" element={<p>Tag setup screen</p>} />
        </Routes>
      </MemoryRouter>
    </PageTitleProvider>,
  )
}

describe('TeamPickerPage', () => {
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

  it('redirects to /tag-setup when the user still needs to set a tag', () => {
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

    expect(screen.getByText('Tag setup screen')).toBeInTheDocument()
  })

  it('renders the home placeholder once tagged', () => {
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

    renderPage()

    expect(screen.getByText('Screens land in later stages.')).toBeInTheDocument()
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
