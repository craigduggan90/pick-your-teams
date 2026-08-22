import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { PageTitleProvider } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { useGame } from '@/hooks/useGame'
import { useSelf } from '@/hooks/useSelf'
import { useUpdateGame } from '@/hooks/useUpdateGame'
import { useDeleteGame } from '@/hooks/useDeleteGame'
import { useRecordResult } from '@/hooks/useRecordResult'
import { GameViewPage } from './GameViewPage'
import type { GameDetailModel } from '@/api/games'

vi.mock('@/hooks/useGame')
vi.mock('@/hooks/useSelf')
vi.mock('@/hooks/useUpdateGame')
vi.mock('@/hooks/useDeleteGame')
vi.mock('@/hooks/useRecordResult')

const organiser = { id: 'organiser-1', tag: 'organiser-tag', displayName: 'The Organiser' }

const scheduledGame: GameDetailModel = {
  id: 'game-1',
  location: 'Oak Leaf Leisure Centre',
  startTime: '2026-08-10T20:00:00.000Z',
  duration: 60,
  teamSize: 5,
  status: 'Scheduled',
  organiser,
  winner: null,
  homeTeamRating: null,
  awayTeamRating: null,
  created: '2026-01-01T00:00:00.000Z',
  modified: '2026-01-01T00:00:00.000Z',
}

const finishedGame: GameDetailModel = {
  ...scheduledGame,
  status: 'Finished',
  winner: 'Home',
  homeTeamRating: 100,
  awayTeamRating: 90,
}

function FooterActionsStub() {
  return <>{useFooterActions()}</>
}

function mockMutations(overrides: { update?: any; del?: any; record?: any } = {}) {
  const updateMutate = vi.fn()
  const deleteMutate = vi.fn()
  const recordMutate = vi.fn()
  vi.mocked(useUpdateGame).mockReturnValue({
    mutate: updateMutate,
    isPending: false,
    isSuccess: false,
    isError: false,
    error: null,
    ...overrides.update,
  } as any)
  vi.mocked(useDeleteGame).mockReturnValue({
    mutate: deleteMutate,
    isPending: false,
    isSuccess: false,
    ...overrides.del,
  } as any)
  vi.mocked(useRecordResult).mockReturnValue({
    mutate: recordMutate,
    isPending: false,
    isSuccess: false,
    ...overrides.record,
  } as any)
  return { updateMutate, deleteMutate, recordMutate }
}

function renderPage() {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <PageActionsProvider>
        <MemoryRouter initialEntries={['/games/game-1']}>
          <Routes>
            <Route path="/games/:id" element={<GameViewPage />} />
            <Route path="/games/:id/teams" element={<p>Teams screen</p>} />
            <Route path="/" element={<p>Games list</p>} />
          </Routes>
          <FooterActionsStub />
        </MemoryRouter>
      </PageActionsProvider>
    </PageTitleProvider>,
  )
}

describe('GameViewPage', () => {
  it('shows a loading state', () => {
    vi.mocked(useGame).mockReturnValue({ isPending: true, isError: false } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: true } as any)
    mockMutations()

    renderPage()

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error state', () => {
    vi.mocked(useGame).mockReturnValue({ isPending: false, isError: true, data: undefined } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, data: organiser } as any)
    mockMutations()

    renderPage()

    expect(screen.getByText('Something went wrong loading this game.')).toBeInTheDocument()
  })

  describe('as the organiser, scheduled', () => {
    function setUp() {
      vi.mocked(useGame).mockReturnValue({
        isPending: false,
        isError: false,
        data: scheduledGame,
      } as any)
      vi.mocked(useSelf).mockReturnValue({ isPending: false, data: { id: 'organiser-1' } } as any)
      return mockMutations()
    }

    it('renders editable fields and all organiser actions', () => {
      setUp()
      renderPage()

      expect(screen.getByLabelText('Location')).toBeEnabled()
      expect(screen.getByLabelText('Duration')).toBeEnabled()
      expect(screen.getByRole('button', { name: 'Manage Teams' })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'Record Result' })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'Delete Game' })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument()
    })

    it('saves edits via the footer Save button', async () => {
      const { updateMutate } = setUp()
      const user = userEvent.setup()
      renderPage()

      await user.clear(screen.getByLabelText('Location'))
      await user.type(screen.getByLabelText('Location'), 'New Location')
      await user.click(screen.getByRole('button', { name: 'Save' }))

      expect(updateMutate).toHaveBeenCalledWith(
        expect.objectContaining({ Location: 'New Location', Duration: 60 }),
      )
    })

    it('opens the record result modal and confirms a winner', async () => {
      const { recordMutate } = setUp()
      const user = userEvent.setup()
      renderPage()

      await user.click(screen.getByRole('button', { name: 'Record Result' }))
      await user.click(screen.getByRole('button', { name: 'Home Team' }))
      await user.click(screen.getByRole('button', { name: 'Confirm' }))

      expect(recordMutate).toHaveBeenCalledWith('Home')
    })

    it('opens the delete confirmation and deletes on confirm', async () => {
      const { deleteMutate } = setUp()
      const user = userEvent.setup()
      renderPage()

      await user.click(screen.getByRole('button', { name: 'Delete Game' }))
      expect(screen.getByText('This cannot be undone.')).toBeInTheDocument()

      // The Dialog primitive marks background content aria-hidden while open, so the page's own
      // trigger button drops out of the accessibility tree here — only the modal's own button is
      // queryable, not a second element to index into.
      await user.click(screen.getByRole('button', { name: 'Delete Game' }))

      expect(deleteMutate).toHaveBeenCalled()
    })
  })

  describe('as a non-organiser, scheduled', () => {
    it('renders read-only fields, View Teams only, no Save', () => {
      vi.mocked(useGame).mockReturnValue({
        isPending: false,
        isError: false,
        data: scheduledGame,
      } as any)
      vi.mocked(useSelf).mockReturnValue({ isPending: false, data: { id: 'someone-else' } } as any)
      mockMutations()

      renderPage()

      expect(screen.getByLabelText('Location')).toBeDisabled()
      expect(screen.queryByRole('button', { name: 'Manage Teams' })).not.toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'View Teams' })).toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Record Result' })).not.toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Delete Game' })).not.toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Save' })).not.toBeInTheDocument()
    })
  })

  describe('finished game', () => {
    it('shows the winner banner, View Teams, and Delete Game but no Save', () => {
      vi.mocked(useGame).mockReturnValue({
        isPending: false,
        isError: false,
        data: finishedGame,
      } as any)
      vi.mocked(useSelf).mockReturnValue({ isPending: false, data: { id: 'organiser-1' } } as any)
      mockMutations()

      renderPage()

      expect(screen.getByText('Winner: Home Team!')).toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'View Teams' })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'Delete Game' })).toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Save' })).not.toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Record Result' })).not.toBeInTheDocument()
      expect(screen.getByLabelText('Location')).toBeDisabled()
    })
  })

  it('navigates to the Teams screen via Manage Teams', async () => {
    vi.mocked(useGame).mockReturnValue({
      isPending: false,
      isError: false,
      data: scheduledGame,
    } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, data: { id: 'organiser-1' } } as any)
    mockMutations()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Manage Teams' }))

    expect(screen.getByText('Teams screen')).toBeInTheDocument()
  })

  it('navigates back to the Teams screen via Back — Game View is reached from there now', async () => {
    vi.mocked(useGame).mockReturnValue({
      isPending: false,
      isError: false,
      data: scheduledGame,
    } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, data: { id: 'organiser-1' } } as any)
    mockMutations()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Back' }))

    expect(screen.getByText('Teams screen')).toBeInTheDocument()
  })
})
