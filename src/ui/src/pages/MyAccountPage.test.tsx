import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router'
import { PageTitleProvider, useHeaderTitle } from '@/hooks/usePageTitle'
import type { ChangeTagLocationState } from '@/lib/navigation'
import { MyAccountPage } from './MyAccountPage'

function HeaderTitleStub() {
  return <h1>{useHeaderTitle()}</h1>
}

function ChangeTagStub() {
  const location = useLocation()
  const state = location.state as ChangeTagLocationState | null
  return (
    <p>
      Change tag screen, from: {state?.from ?? '(none)'}
    </p>
  )
}

function renderPage() {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <HeaderTitleStub />
      <MemoryRouter initialEntries={['/account']}>
        <Routes>
          <Route path="/account" element={<MyAccountPage />} />
          <Route path="/change-tag" element={<ChangeTagStub />} />
        </Routes>
      </MemoryRouter>
    </PageTitleProvider>,
  )
}

describe('MyAccountPage', () => {
  it('sets the header title to My Account', () => {
    renderPage()
    expect(screen.getByRole('heading')).toHaveTextContent('My Account')
  })

  it('navigates to /change-tag with the current path as the return state', async () => {
    const user = userEvent.setup()
    renderPage()

    await user.click(screen.getByRole('button', { name: 'Change Tag' }))

    expect(screen.getByText('Change tag screen, from: /account')).toBeInTheDocument()
  })
})
