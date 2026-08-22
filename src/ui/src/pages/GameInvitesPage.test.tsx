import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { PageTitleProvider } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { useGame } from '@/hooks/useGame'
import { useSelf } from '@/hooks/useSelf'
import { useInvitations } from '@/hooks/useInvitations'
import { GameInvitesPage } from './GameInvitesPage'
import type { GameDetailModel } from '@/api/games'
import type { InvitationModel } from '@/api/invitations'

vi.mock('@/hooks/useGame')
vi.mock('@/hooks/useSelf')
vi.mock('@/hooks/useInvitations')

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

const invitation1: InvitationModel = {
  id: 'inv-1',
  status: 'Open',
  game: { id: 'game-1', startTime: scheduledGame.startTime, duration: 60, location: scheduledGame.location },
  organiser,
  invitee: { id: 'user-2', tag: 'monkey-duster', displayName: 'Jordan Monk' },
  created: '2026-08-01T09:00:00.000Z',
}

const invitation2: InvitationModel = {
  id: 'inv-2',
  status: 'Accepted',
  game: { id: 'game-1', startTime: scheduledGame.startTime, duration: 60, location: scheduledGame.location },
  organiser,
  invitee: { id: 'user-3', tag: 'f30', displayName: 'Sam Forster' },
  created: '2026-08-02T09:00:00.000Z',
}

function FooterActionsStub() {
  return <>{useFooterActions()}</>
}

function mockInvitations(overrides: object = {}) {
  vi.mocked(useInvitations).mockReturnValue({
    isPending: false,
    isError: false,
    data: { pages: [{ data: [invitation1, invitation2], cursor: null, count: 2 }] },
    hasNextPage: false,
    isFetchingNextPage: false,
    fetchNextPage: vi.fn(),
    ...overrides,
  } as any)
}

function renderPage() {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <PageActionsProvider>
        <MemoryRouter initialEntries={['/games/game-1/invites']}>
          <Routes>
            <Route path="/games/:id/invites" element={<GameInvitesPage />} />
            <Route path="/games/:id" element={<p>Game view screen</p>} />
            <Route path="/games/:id/teams" element={<p>Teams screen</p>} />
          </Routes>
          <FooterActionsStub />
        </MemoryRouter>
      </PageActionsProvider>
    </PageTitleProvider>,
  )
}

describe('GameInvitesPage', () => {
  it('shows a loading state', () => {
    vi.mocked(useGame).mockReturnValue({ isPending: true, isError: false, data: undefined } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: true, data: undefined } as any)
    mockInvitations({ isPending: true, data: undefined })

    renderPage()

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error state when the game fails to load', () => {
    vi.mocked(useGame).mockReturnValue({ isPending: false, isError: true, data: undefined } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, data: organiser } as any)
    mockInvitations()

    renderPage()

    expect(screen.getByText('Something went wrong loading this game.')).toBeInTheDocument()
  })

  it('redirects a non-organiser back to the teams screen', () => {
    vi.mocked(useGame).mockReturnValue({ isPending: false, isError: false, data: scheduledGame } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, data: { id: 'someone-else' } } as any)
    mockInvitations({ isPending: true, data: undefined })

    renderPage()

    expect(screen.getByText('Teams screen')).toBeInTheDocument()
  })

  describe('as the organiser', () => {
    function setUp() {
      vi.mocked(useGame).mockReturnValue({ isPending: false, isError: false, data: scheduledGame } as any)
      vi.mocked(useSelf).mockReturnValue({ isPending: false, data: organiser } as any)
    }

    it('shows an error state when invitations fail to load', () => {
      setUp()
      mockInvitations({ isError: true, isPending: false, data: undefined })

      renderPage()

      expect(screen.getByText('Something went wrong loading these invites.')).toBeInTheDocument()
    })

    it('shows an empty state when no invitations have been sent', () => {
      setUp()
      mockInvitations({ data: { pages: [{ data: [], cursor: null, count: 0 }] } })

      renderPage()

      expect(screen.getByText('No Invites Sent Yet!')).toBeInTheDocument()
    })

    it('renders each invitation with the invitee and status', () => {
      setUp()
      mockInvitations()

      renderPage()

      expect(screen.getByText('Jordan Monk (@monkey-duster)')).toBeInTheDocument()
      expect(screen.getByText('Pending')).toBeInTheDocument()
      expect(screen.getByText('Sam Forster (@f30)')).toBeInTheDocument()
      expect(screen.getByText('Accepted')).toBeInTheDocument()
    })

    it('shows Load More when another page is available', async () => {
      setUp()
      const fetchNextPage = vi.fn()
      mockInvitations({ hasNextPage: true, fetchNextPage })
      const user = userEvent.setup()

      renderPage()
      await user.click(screen.getByRole('button', { name: 'Load More…' }))

      expect(fetchNextPage).toHaveBeenCalled()
    })

    it('Back returns to the game view screen', async () => {
      setUp()
      mockInvitations()
      const user = userEvent.setup()

      renderPage()
      await user.click(screen.getByRole('button', { name: 'Back' }))

      expect(screen.getByText('Game view screen')).toBeInTheDocument()
    })
  })
})
