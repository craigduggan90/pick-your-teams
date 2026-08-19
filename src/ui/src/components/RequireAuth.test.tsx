import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { useAuth0 } from '@auth0/auth0-react'
import { RequireAuth } from './RequireAuth'

vi.mock('@auth0/auth0-react')

function renderGuard() {
  return render(
    <MemoryRouter initialEntries={['/protected']}>
      <Routes>
        <Route
          path="/protected"
          element={
            <RequireAuth>
              <p>Protected content</p>
            </RequireAuth>
          }
        />
        <Route path="/" element={<p>Team Picker landing</p>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('RequireAuth', () => {
  it('shows a loading state while auth0 is resolving', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: false,
      isLoading: true,
    } as unknown as ReturnType<typeof useAuth0>)

    renderGuard()

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('redirects to / when not authenticated', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
    } as unknown as ReturnType<typeof useAuth0>)

    renderGuard()

    expect(screen.getByText('Team Picker landing')).toBeInTheDocument()
  })

  it('renders children when authenticated', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
    } as unknown as ReturnType<typeof useAuth0>)

    renderGuard()

    expect(screen.getByText('Protected content')).toBeInTheDocument()
  })
})
