import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { useAuth0 } from '@auth0/auth0-react'
import { Header } from './Header'

vi.mock('@auth0/auth0-react')

function renderHeader() {
  return render(
    <MemoryRouter initialEntries={['/games']}>
      <Routes>
        <Route path="/games" element={<Header title="Games" />} />
        <Route path="/" element={<p>Home page</p>} />
        <Route path="/account" element={<p>My account page</p>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('Header', () => {
  it('renders the screen title', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: true,
    } as unknown as ReturnType<typeof useAuth0>)

    renderHeader()

    expect(screen.getByRole('heading', { name: 'Games' })).toBeInTheDocument()
  })

  describe('when authenticated', () => {
    it('navigates home when the left icon is clicked', async () => {
      vi.mocked(useAuth0).mockReturnValue({
        isAuthenticated: true,
      } as unknown as ReturnType<typeof useAuth0>)
      const user = userEvent.setup()
      renderHeader()

      await user.click(screen.getByRole('button', { name: 'Home' }))

      expect(screen.getByText('Home page')).toBeInTheDocument()
    })

    it('shows a My Account icon that navigates to /account', async () => {
      vi.mocked(useAuth0).mockReturnValue({
        isAuthenticated: true,
      } as unknown as ReturnType<typeof useAuth0>)
      const user = userEvent.setup()
      renderHeader()

      expect(screen.getByTestId('account-icon')).toBeInTheDocument()
      await user.click(screen.getByRole('button', { name: 'My Account' }))

      expect(screen.getByText('My account page')).toBeInTheDocument()
    })
  })

  describe('when not authenticated', () => {
    it('shows the home icon but not as a link/button', () => {
      vi.mocked(useAuth0).mockReturnValue({
        isAuthenticated: false,
      } as unknown as ReturnType<typeof useAuth0>)

      renderHeader()

      expect(screen.getByTestId('home-icon')).toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Home' })).not.toBeInTheDocument()
    })

    it('hides the My Account icon entirely', () => {
      vi.mocked(useAuth0).mockReturnValue({
        isAuthenticated: false,
      } as unknown as ReturnType<typeof useAuth0>)

      renderHeader()

      expect(screen.queryByRole('button', { name: 'My Account' })).not.toBeInTheDocument()
    })
  })
})
