import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { PageTitleProvider } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { useSelf } from '@/hooks/useSelf'
import { useCreateGame } from '@/hooks/useCreateGame'
import { nextHourStart } from '@/lib/format'
import { NewGamePage } from './NewGamePage'

vi.mock('@/hooks/useSelf')
vi.mock('@/hooks/useCreateGame')

// Same computation NewGamePage uses for its default — comparing against a freshly computed value
// rather than a fixed string avoids the test being time-dependent/flaky.
const nextHourStartLocalValue = nextHourStart

function FooterActionsStub() {
  return <>{useFooterActions()}</>
}

function renderPage() {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <PageActionsProvider>
        <MemoryRouter initialEntries={['/games/new']}>
          <Routes>
            <Route path="/games/new" element={<NewGamePage />} />
            <Route path="/" element={<p>Games list</p>} />
            <Route path="/games/:id" element={<p>Game view screen</p>} />
          </Routes>
          <FooterActionsStub />
        </MemoryRouter>
      </PageActionsProvider>
    </PageTitleProvider>,
  )
}

function mockCreateGame(overrides: any = {}) {
  const mutate = vi.fn()
  vi.mocked(useCreateGame).mockReturnValue({
    mutate,
    isPending: false,
    isSuccess: false,
    isError: false,
    error: null,
    data: undefined,
    ...overrides,
  } as any)
  return mutate
}

describe('NewGamePage', () => {
  it('defaults Start Time to the top of the next hour, pre-filled and enabled', () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockCreateGame()

    renderPage()

    expect(screen.getByLabelText('Start Time')).toHaveValue(nextHourStartLocalValue())
    expect(screen.getByRole('button', { name: 'Create' })).toBeEnabled()
  })

  it('disables Create when Start Time is cleared', async () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockCreateGame()
    const user = userEvent.setup()

    renderPage()
    await user.clear(screen.getByLabelText('Start Time'))

    expect(screen.getByRole('button', { name: 'Create' })).toBeDisabled()
  })

  it('creates the game with the current user as organiser and the default start time', async () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    const mutate = mockCreateGame()
    const user = userEvent.setup()

    renderPage()
    await user.clear(screen.getByLabelText('Location'))
    await user.type(screen.getByLabelText('Location'), 'The Pitch')
    await user.click(screen.getByRole('button', { name: 'Create' }))

    expect(mutate).toHaveBeenCalledWith(
      expect.objectContaining({
        Location: 'The Pitch',
        Duration: 60,
        TeamSize: 5,
        OrganiserId: 'user-1',
        StartTime: expect.stringMatching(/^\d{4}-\d{2}-\d{2}T\d{2}:00:00\.000Z$/),
      }),
    )
  })

  it('navigates to the new game on success', async () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockCreateGame({ isSuccess: true, data: { id: 'game-1' } })

    renderPage()

    expect(await screen.findByText('Game view screen')).toBeInTheDocument()
  })

  it('cancels back to the games list', async () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockCreateGame()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(screen.getByText('Games list')).toBeInTheDocument()
  })
})
