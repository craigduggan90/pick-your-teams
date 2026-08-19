import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { useAuth0 } from '@auth0/auth0-react'
import { useSelf } from '@/hooks/useSelf'
import { RequireAuthAndTag } from './RequireAuthAndTag'

vi.mock('@auth0/auth0-react')
vi.mock('@/hooks/useSelf')

function renderGuard() {
  return render(
    <MemoryRouter initialEntries={['/protected']}>
      <Routes>
        <Route
          path="/protected"
          element={
            <RequireAuthAndTag>
              <p>Protected content</p>
            </RequireAuthAndTag>
          }
        />
        <Route path="/" element={<p>Team Picker landing</p>} />
        <Route path="/change-tag" element={<p>Change tag screen</p>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('RequireAuthAndTag', () => {
  it('redirects to / when not authenticated', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
    } as unknown as ReturnType<typeof useAuth0>)

    renderGuard()

    expect(screen.getByText('Team Picker landing')).toBeInTheDocument()
  })

  it('redirects to /change-tag when authenticated but untagged', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
    } as unknown as ReturnType<typeof useAuth0>)
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: '1', tag: '1' },
    } as unknown as ReturnType<typeof useSelf>)

    renderGuard()

    expect(screen.getByText('Change tag screen')).toBeInTheDocument()
  })

  it('renders children when authenticated and tagged', () => {
    vi.mocked(useAuth0).mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
    } as unknown as ReturnType<typeof useAuth0>)
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: '1', tag: 'bob' },
    } as unknown as ReturnType<typeof useSelf>)

    renderGuard()

    expect(screen.getByText('Protected content')).toBeInTheDocument()
  })
})
