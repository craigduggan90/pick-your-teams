import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { PageTitleProvider } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { useGame } from '@/hooks/useGame'
import { useSelf } from '@/hooks/useSelf'
import { useCreateInvitations } from '@/hooks/useCreateInvitations'
import { InvitePlayersPage } from './InvitePlayersPage'
import type { GameDetailModel } from '@/api/games'
import { ApiError } from '@/api/client'
import { toast } from '@/components/Toast'

vi.mock('@/components/Toast', () => ({ toast: { success: vi.fn(), error: vi.fn() } }))
vi.mock('@/hooks/useGame')
vi.mock('@/hooks/useSelf')
vi.mock('@/hooks/useCreateInvitations')

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

function FooterActionsStub() {
  return <>{useFooterActions()}</>
}

function mockCreateInvitations(overrides: object = {}) {
  const mutate = vi.fn()
  vi.mocked(useCreateInvitations).mockReturnValue({
    mutate,
    isPending: false,
    isSuccess: false,
    isError: false,
    error: null,
    ...overrides,
  } as any)
  return mutate
}

function renderPage() {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <PageActionsProvider>
        <MemoryRouter initialEntries={['/games/game-1/invite']}>
          <Routes>
            <Route path="/games/:id/invite" element={<InvitePlayersPage />} />
            <Route path="/games/:id/teams" element={<p>Teams screen</p>} />
          </Routes>
          <FooterActionsStub />
        </MemoryRouter>
      </PageActionsProvider>
    </PageTitleProvider>,
  )
}

describe('InvitePlayersPage', () => {
  it('shows a loading state', () => {
    vi.mocked(useGame).mockReturnValue({ isPending: true, isError: false, data: undefined } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: true, data: undefined } as any)
    mockCreateInvitations()

    renderPage()

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error state', () => {
    vi.mocked(useGame).mockReturnValue({ isPending: false, isError: true, data: undefined } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, data: organiser } as any)
    mockCreateInvitations()

    renderPage()

    expect(screen.getByText('Something went wrong loading this game.')).toBeInTheDocument()
  })

  it('redirects a non-organiser back to the teams screen', () => {
    vi.mocked(useGame).mockReturnValue({ isPending: false, isError: false, data: scheduledGame } as any)
    vi.mocked(useSelf).mockReturnValue({ isPending: false, data: { id: 'someone-else' } } as any)
    mockCreateInvitations()

    renderPage()

    expect(screen.getByText('Teams screen')).toBeInTheDocument()
  })

  describe('as the organiser', () => {
    function setUp() {
      vi.mocked(useGame).mockReturnValue({ isPending: false, isError: false, data: scheduledGame } as any)
      vi.mocked(useSelf).mockReturnValue({ isPending: false, data: organiser } as any)
      return mockCreateInvitations()
    }

    it('starts with a single empty Tag field and Send disabled', () => {
      setUp()
      renderPage()

      expect(screen.getAllByLabelText('Tag')).toHaveLength(1)
      expect(screen.getByRole('button', { name: 'Send Invitations' })).toBeDisabled()
      expect(screen.queryByRole('button', { name: 'Remove tag' })).not.toBeInTheDocument()
    })

    it('enables Send once a tag is entered and sends the trimmed tags', async () => {
      const mutate = setUp()
      const user = userEvent.setup()
      renderPage()

      await user.type(screen.getByLabelText('Tag'), '  monkey-duster  ')
      expect(screen.getByRole('button', { name: 'Send Invitations' })).toBeEnabled()

      await user.click(screen.getByRole('button', { name: 'Send Invitations' }))

      expect(mutate).toHaveBeenCalledWith(['monkey-duster'])
    })

    it('adds and removes tag rows, only sending non-empty ones', async () => {
      const mutate = setUp()
      const user = userEvent.setup()
      renderPage()

      await user.click(screen.getByRole('button', { name: '+ Add Another Tag' }))
      expect(screen.getAllByLabelText('Tag')).toHaveLength(2)

      const tagInputs = screen.getAllByLabelText('Tag')
      await user.type(tagInputs[0], 'monkey-duster')

      await user.click(screen.getByRole('button', { name: 'Send Invitations' }))

      expect(mutate).toHaveBeenCalledWith(['monkey-duster'])

      await user.click(screen.getByRole('button', { name: '+ Add Another Tag' }))
      expect(screen.getAllByRole('button', { name: 'Remove tag' })).toHaveLength(3)
      await user.click(screen.getAllByRole('button', { name: 'Remove tag' })[0])
      expect(screen.getAllByLabelText('Tag')).toHaveLength(2)
    })

    it('renders every error message as a flat list, regardless of key', () => {
      setUp()
      vi.mocked(useCreateInvitations).mockReturnValue({
        mutate: vi.fn(),
        isPending: false,
        isSuccess: false,
        isError: true,
        error: new ApiError(422, {
          detail: 'One or more validation failures occurred.',
          errors: {
            '': ["Tag not found: ghost-tag"],
            UserTags: ['Duplicate tags provided.'],
          },
        }),
      } as any)

      renderPage()

      expect(screen.getByText('Tag not found: ghost-tag')).toBeInTheDocument()
      expect(screen.getByText('Duplicate tags provided.')).toBeInTheDocument()
    })

    it('toasts a generic message for an error with no field-level errors', () => {
      setUp()
      vi.mocked(useCreateInvitations).mockReturnValue({
        mutate: vi.fn(),
        isPending: false,
        isSuccess: false,
        isError: true,
        error: new ApiError(403, { detail: 'You are not the organiser of this game.' }),
      } as any)

      renderPage()

      expect(toast.error).toHaveBeenCalledWith('You are not the organiser of this game.')
    })

    it('navigates back to the teams screen and toasts on success', () => {
      setUp()
      vi.mocked(useCreateInvitations).mockReturnValue({
        mutate: vi.fn(),
        isPending: false,
        isSuccess: true,
        isError: false,
        error: null,
      } as any)

      renderPage()

      expect(toast.success).toHaveBeenCalledWith('Invitations sent!')
      expect(screen.getByText('Teams screen')).toBeInTheDocument()
    })

    it('Cancel returns to the teams screen', async () => {
      setUp()
      const user = userEvent.setup()
      renderPage()

      await user.click(screen.getByRole('button', { name: 'Cancel' }))

      expect(screen.getByText('Teams screen')).toBeInTheDocument()
    })
  })
})
