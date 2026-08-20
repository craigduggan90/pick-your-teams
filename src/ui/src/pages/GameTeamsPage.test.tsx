import { describe, expect, it, vi } from 'vitest'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { PageTitleProvider } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { useGame } from '@/hooks/useGame'
import { useSelf } from '@/hooks/useSelf'
import { useGameTeams } from '@/hooks/useGameTeams'
import { useSetGameTeams } from '@/hooks/useSetGameTeams'
import { useGenerateGameTeams } from '@/hooks/useGenerateGameTeams'
import { useCreateDummyPlayer } from '@/hooks/useCreateDummyPlayer'
import { useDeletePlayer } from '@/hooks/useDeletePlayer'
import { GameTeamsPage } from './GameTeamsPage'
import type { GameDetailModel, GameTeamsModel } from '@/api/games'
import { ApiError } from '@/api/client'

vi.mock('@/hooks/useGame')
vi.mock('@/hooks/useSelf')
vi.mock('@/hooks/useGameTeams')
vi.mock('@/hooks/useSetGameTeams')
vi.mock('@/hooks/useGenerateGameTeams')
vi.mock('@/hooks/useCreateDummyPlayer')
vi.mock('@/hooks/useDeletePlayer')

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

const finishedGame: GameDetailModel = { ...scheduledGame, status: 'Finished' }

const homePlayer = { id: 'p-home', displayName: 'Home Player', tag: 'homeplayer', rating: 900 }
const awayPlayer = { id: 'p-away', displayName: 'Away Player', tag: 'awayplayer', rating: 850 }
const benchPlayer = { id: 'p-bench', displayName: 'Bench Player', tag: 'benchplayer', rating: 700 }
const dummyPlayer = { id: 'p-dummy', displayName: 'Dummy Player', tag: null, rating: 500 }

const teamsFixture: GameTeamsModel = {
  id: 'game-1',
  home: { players: [homePlayer], teamRating: 900 },
  away: { players: [awayPlayer], teamRating: 850 },
  unassigned: [benchPlayer, dummyPlayer],
}

function FooterActionsStub() {
  return <>{useFooterActions()}</>
}

function mockMutations(
  overrides: {
    setTeams?: object
    generate?: object
    createDummy?: object
    deletePlayer?: object
  } = {},
) {
  const setTeamsMutate = vi.fn()
  const generateMutate = vi.fn()
  const createDummyMutate = vi.fn()
  const deleteMutate = vi.fn()
  vi.mocked(useSetGameTeams).mockReturnValue({
    mutate: setTeamsMutate,
    isPending: false,
    isSuccess: false,
    isError: false,
    error: null,
    ...overrides.setTeams,
  } as any)
  vi.mocked(useGenerateGameTeams).mockReturnValue({
    mutate: generateMutate,
    isPending: false,
    isSuccess: false,
    isError: false,
    error: null,
    data: undefined,
    ...overrides.generate,
  } as any)
  vi.mocked(useCreateDummyPlayer).mockReturnValue({
    mutate: createDummyMutate,
    isPending: false,
    isSuccess: false,
    isError: false,
    error: null,
    ...overrides.createDummy,
  } as any)
  vi.mocked(useDeletePlayer).mockReturnValue({
    mutate: deleteMutate,
    isPending: false,
    isSuccess: false,
    isError: false,
    error: null,
    ...overrides.deletePlayer,
  } as any)
  return { setTeamsMutate, generateMutate, createDummyMutate, deleteMutate }
}

function setUp(game: GameDetailModel, selfId: string, teams: GameTeamsModel = teamsFixture) {
  vi.mocked(useGame).mockReturnValue({ isPending: false, isError: false, data: game } as any)
  vi.mocked(useSelf).mockReturnValue({ isPending: false, data: { id: selfId } } as any)
  vi.mocked(useGameTeams).mockReturnValue({ isPending: false, isError: false, data: teams } as any)
  return mockMutations()
}

function renderPage() {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <PageActionsProvider>
        <MemoryRouter initialEntries={['/games/game-1/teams']}>
          <Routes>
            <Route path="/games/:id/teams" element={<GameTeamsPage />} />
            <Route path="/games/:id" element={<p>Game view</p>} />
          </Routes>
          <FooterActionsStub />
        </MemoryRouter>
      </PageActionsProvider>
    </PageTitleProvider>,
  )
}

async function pickAction(playerId: string, label: string, user: ReturnType<typeof userEvent.setup>) {
  const row = within(screen.getByTestId(`team-roster-row-${playerId}`))
  await user.click(row.getByRole('combobox'))
  await user.click(await screen.findByRole('option', { name: label }))
}

describe('GameTeamsPage', () => {
  it('shows a loading state', () => {
    vi.mocked(useGame).mockReturnValue({ isPending: true, isError: false } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: true } as any)
    vi.mocked(useGameTeams).mockReturnValue({ isPending: true, isError: false } as any)
    mockMutations()

    renderPage()

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error state', () => {
    vi.mocked(useGame).mockReturnValue({ isPending: false, isError: true, data: undefined } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, data: organiser } as any)
    vi.mocked(useGameTeams).mockReturnValue({ isPending: false, isError: true, data: undefined } as any)
    mockMutations()

    renderPage()

    expect(screen.getByText('Something went wrong loading these teams.')).toBeInTheDocument()
  })

  describe('read-only (non-organiser)', () => {
    it('renders rosters with no editing controls', () => {
      setUp(scheduledGame, 'someone-else')

      renderPage()

      expect(screen.getByText('Home Player')).toBeInTheDocument()
      expect(screen.getByText('Away Player')).toBeInTheDocument()
      expect(screen.getByText('Bench Player')).toBeInTheDocument()
      expect(screen.queryByRole('combobox')).not.toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Reset' })).not.toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Generate' })).not.toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Save' })).not.toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'Back' })).toBeInTheDocument()
    })
  })

  describe('read-only (finished game, even for the organiser)', () => {
    it('renders rosters with no editing controls', () => {
      setUp(finishedGame, 'organiser-1')

      renderPage()

      expect(screen.queryByRole('combobox')).not.toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Save' })).not.toBeInTheDocument()
    })
  })

  describe('editable (organiser, scheduled)', () => {
    it('renders all editing controls', () => {
      setUp(scheduledGame, 'organiser-1')

      renderPage()

      expect(screen.getByRole('button', { name: 'Reset' })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'Generate' })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'Add Non-User Player' })).toBeInTheDocument()
    })

    it('moving a player to a team stays pending — no Save call yet', async () => {
      const { setTeamsMutate } = setUp(scheduledGame, 'organiser-1')
      const user = userEvent.setup()
      renderPage()

      await pickAction('p-bench', 'To Home Team', user)

      expect(setTeamsMutate).not.toHaveBeenCalled()
      // The row now renders under the Home section's roster (re-mounted there), not Unassigned.
      const homeSection = screen.getByRole('heading', { name: 'Home Team' }).closest('div')!
        .parentElement!
      expect(within(homeSection).getByText('Bench Player')).toBeInTheDocument()
    })

    it('Save sends the merged Home/Away ids, including pending moves', async () => {
      const { setTeamsMutate } = setUp(scheduledGame, 'organiser-1')
      const user = userEvent.setup()
      renderPage()

      await pickAction('p-bench', 'To Home Team', user)
      await user.click(screen.getByRole('button', { name: 'Save' }))

      expect(setTeamsMutate).toHaveBeenCalledWith({
        HomeTeamIds: ['p-home', 'p-bench'],
        AwayTeamIds: ['p-away'],
      })
    })

    it('Reset discards a pending move', async () => {
      const { setTeamsMutate } = setUp(scheduledGame, 'organiser-1')
      const user = userEvent.setup()
      renderPage()

      await pickAction('p-bench', 'To Home Team', user)
      await user.click(screen.getByRole('button', { name: 'Reset' }))
      await user.click(screen.getByRole('button', { name: 'Save' }))

      expect(setTeamsMutate).toHaveBeenCalledWith({
        HomeTeamIds: ['p-home'],
        AwayTeamIds: ['p-away'],
      })
    })

    it('Generate seeds from the last-saved Home/Away split', async () => {
      const { generateMutate } = setUp(scheduledGame, 'organiser-1')
      const user = userEvent.setup()
      renderPage()

      await user.click(screen.getByRole('button', { name: 'Generate' }))

      expect(generateMutate).toHaveBeenCalledWith({
        homeTeamSeedIds: ['p-home'],
        awayTeamSeedIds: ['p-away'],
      })
    })

    it('only marks players Generate actually moved as pending, not ones already seeded there', () => {
      // Generate rebuilds the overlay from every player in its response, including ones whose
      // seeded position didn't change (p-home, p-away) — only p-bench actually moved (Unassigned
      // -> Home). Pending must be judged against the last-saved bucket, not overlay presence, or
      // every returned player — seeded or not — would show as pending.
      setUp(scheduledGame, 'organiser-1')
      vi.mocked(useGenerateGameTeams).mockReturnValue({
        mutate: vi.fn(),
        isPending: false,
        isSuccess: true,
        isError: false,
        error: null,
        data: {
          id: 'game-1',
          home: { players: [homePlayer, benchPlayer], teamRating: 1600 },
          away: { players: [awayPlayer], teamRating: 850 },
          unassigned: [dummyPlayer],
        },
      } as any)

      renderPage()

      expect(screen.getByTestId('team-roster-row-p-home')).toHaveClass('border-primary')
      expect(screen.getByTestId('team-roster-row-p-home')).not.toHaveClass('border-primary/30')
      expect(screen.getByTestId('team-roster-row-p-bench')).toHaveClass('border-primary/30')
      expect(screen.getByTestId('team-roster-row-p-away')).toHaveClass('border-secondary')
      expect(screen.getByTestId('team-roster-row-p-away')).not.toHaveClass('border-secondary/30')
    })

    it('Remove from Game shows a confirmation modal for a tagged (User) player', async () => {
      const { deleteMutate } = setUp(scheduledGame, 'organiser-1')
      const user = userEvent.setup()
      renderPage()

      await pickAction('p-bench', 'Remove from Game', user)

      expect(screen.getByText('Remove @benchplayer?')).toBeInTheDocument()
      expect(deleteMutate).not.toHaveBeenCalled()

      await user.click(screen.getByRole('button', { name: 'Remove' }))

      expect(deleteMutate).toHaveBeenCalledWith('p-bench')
    })

    it('Remove from Game deletes immediately for a Dummy player, no modal', async () => {
      const { deleteMutate } = setUp(scheduledGame, 'organiser-1')
      const user = userEvent.setup()
      renderPage()

      await pickAction('p-dummy', 'Remove from Game', user)

      expect(deleteMutate).toHaveBeenCalledWith('p-dummy')
      expect(screen.queryByText(/^Remove @/)).not.toBeInTheDocument()
    })

    it('adds a non-user player via the inline form', async () => {
      const { createDummyMutate } = setUp(scheduledGame, 'organiser-1')
      const user = userEvent.setup()
      renderPage()

      await user.click(screen.getByRole('button', { name: 'Add Non-User Player' }))
      await user.type(screen.getByLabelText('Display Name'), 'Mike Rotch')
      await user.clear(screen.getByLabelText('Rating'))
      await user.type(screen.getByLabelText('Rating'), '1000')
      await user.click(screen.getByRole('button', { name: 'Add' }))

      expect(createDummyMutate).toHaveBeenCalledWith({
        displayName: 'Mike Rotch',
        estimatedRating: 1000,
      })
    })

    it('renders an inline field error from a validation failure', async () => {
      setUp(scheduledGame, 'organiser-1', teamsFixture)
      vi.mocked(useCreateDummyPlayer).mockReturnValue({
        mutate: vi.fn(),
        isPending: false,
        isSuccess: false,
        isError: true,
        error: new ApiError(422, { errors: { DisplayName: ['Display name is required.'] } }),
      } as any)
      const user = userEvent.setup()
      renderPage()

      await user.click(screen.getByRole('button', { name: 'Add Non-User Player' }))

      expect(screen.getByText('Display name is required.')).toBeInTheDocument()
    })
  })
})
