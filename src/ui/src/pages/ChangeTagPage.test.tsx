import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { useSelf } from '@/hooks/useSelf'
import { PageTitleProvider, useHeaderTitle } from '@/hooks/usePageTitle'
import { ChangeTagPage } from './ChangeTagPage'

vi.mock('@/hooks/useSelf')
vi.mock('@/components/ChangeTag', () => ({
  ChangeTag: ({
    mode,
    currentTag,
    onSuccess,
    onCancel,
  }: {
    mode: string
    currentTag?: string
    onSuccess?: () => void
    onCancel?: () => void
  }) => (
    <div>
      <p>mode: {mode}</p>
      <p>currentTag: {currentTag ?? '(none)'}</p>
      <button onClick={onSuccess}>trigger success</button>
      <button onClick={onCancel}>trigger cancel</button>
    </div>
  ),
}))

function HeaderTitleStub() {
  return <h1>{useHeaderTitle()}</h1>
}

function renderAt(entry: { pathname: string; state?: unknown }) {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <HeaderTitleStub />
      <MemoryRouter initialEntries={[entry]}>
        <Routes>
          <Route path="/change-tag" element={<ChangeTagPage />} />
          <Route path="/" element={<p>Home page</p>} />
          <Route path="/account" element={<p>My account page</p>} />
        </Routes>
      </MemoryRouter>
    </PageTitleProvider>,
  )
}

describe('ChangeTagPage', () => {
  it('shows a loading state while self is resolving', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: true,
    } as unknown as ReturnType<typeof useSelf>)

    renderAt({ pathname: '/change-tag' })

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error state if self fails to load', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: true,
    } as unknown as ReturnType<typeof useSelf>)

    renderAt({ pathname: '/change-tag' })

    expect(screen.getByText('Something went wrong loading your account.')).toBeInTheDocument()
  })

  it('renders gate mode with no prefilled tag, titled "Set Your Tag"', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: 'user-1', tag: 'user-1' },
    } as unknown as ReturnType<typeof useSelf>)

    renderAt({ pathname: '/change-tag' })

    expect(screen.getByText('mode: gate')).toBeInTheDocument()
    expect(screen.getByText('currentTag: (none)')).toBeInTheDocument()
    expect(screen.getByRole('heading')).toHaveTextContent('Set Your Tag')
  })

  it('renders normal mode prefilled with the current tag, titled "Change Tag"', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: 'user-1', tag: 'bob' },
    } as unknown as ReturnType<typeof useSelf>)

    renderAt({ pathname: '/change-tag' })

    expect(screen.getByText('mode: normal')).toBeInTheDocument()
    expect(screen.getByText('currentTag: bob')).toBeInTheDocument()
    expect(screen.getByRole('heading')).toHaveTextContent('Change Tag')
  })

  it('sends the user back to / by default on success', async () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: 'user-1', tag: 'user-1' },
    } as unknown as ReturnType<typeof useSelf>)
    const user = userEvent.setup()

    renderAt({ pathname: '/change-tag' })
    await user.click(screen.getByRole('button', { name: 'trigger success' }))

    expect(screen.getByText('Home page')).toBeInTheDocument()
  })

  it('sends the user back to where they came from on success', async () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: 'user-1', tag: 'bob' },
    } as unknown as ReturnType<typeof useSelf>)
    const user = userEvent.setup()

    renderAt({ pathname: '/change-tag', state: { from: '/account' } })
    await user.click(screen.getByRole('button', { name: 'trigger success' }))

    expect(screen.getByText('My account page')).toBeInTheDocument()
  })

  it('sends the user back to where they came from on cancel', async () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: 'user-1', tag: 'bob' },
    } as unknown as ReturnType<typeof useSelf>)
    const user = userEvent.setup()

    renderAt({ pathname: '/change-tag', state: { from: '/account' } })
    await user.click(screen.getByRole('button', { name: 'trigger cancel' }))

    expect(screen.getByText('My account page')).toBeInTheDocument()
  })
})
