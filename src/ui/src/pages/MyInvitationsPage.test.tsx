import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { PageTitleProvider } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { useSelf } from '@/hooks/useSelf'
import { useInvitations } from '@/hooks/useInvitations'
import { useAcceptInvitation } from '@/hooks/useAcceptInvitation'
import { useDeclineInvitation } from '@/hooks/useDeclineInvitation'
import { MyInvitationsPage } from './MyInvitationsPage'
import type { InvitationModel } from '@/api/invitations'
import { ApiError } from '@/api/client'
import { toast } from '@/components/Toast'

vi.mock('@/components/Toast', () => ({ toast: { success: vi.fn(), error: vi.fn() } }))
vi.mock('@/hooks/useSelf')
vi.mock('@/hooks/useInvitations')
vi.mock('@/hooks/useAcceptInvitation')
vi.mock('@/hooks/useDeclineInvitation')

const invitation1: InvitationModel = {
  id: 'inv-1',
  status: 'Open',
  game: { id: 'game-1', startTime: '2026-08-10T20:00:00.000Z', duration: 60, location: 'Oak Leaf Leisure Centre' },
  organiser: { id: 'organiser-1', tag: 'the-organiser', displayName: 'The Organiser' },
}

const invitation2: InvitationModel = {
  id: 'inv-2',
  status: 'Open',
  game: { id: 'game-2', startTime: '2026-08-12T18:00:00.000Z', duration: 90, location: 'The Pitch' },
  organiser: { id: 'organiser-2', tag: 'another-organiser', displayName: 'Another Organiser' },
}

function FooterActionsStub() {
  return <>{useFooterActions()}</>
}

function mockMutations(overrides: { accept?: object; decline?: object } = {}) {
  const acceptMutate = vi.fn()
  const declineMutate = vi.fn()
  vi.mocked(useAcceptInvitation).mockReturnValue({
    mutate: acceptMutate,
    isPending: false,
    isError: false,
    error: null,
    variables: undefined,
    ...overrides.accept,
  } as any)
  vi.mocked(useDeclineInvitation).mockReturnValue({
    mutate: declineMutate,
    isPending: false,
    isError: false,
    error: null,
    variables: undefined,
    ...overrides.decline,
  } as any)
  return { acceptMutate, declineMutate }
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
        <MemoryRouter initialEntries={['/invitations']}>
          <Routes>
            <Route path="/invitations" element={<MyInvitationsPage />} />
            <Route path="/" element={<p>Games list</p>} />
          </Routes>
          <FooterActionsStub />
        </MemoryRouter>
      </PageActionsProvider>
    </PageTitleProvider>,
  )
}

describe('MyInvitationsPage', () => {
  it('shows a loading state while self is pending', () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: true, isError: false, data: undefined } as any)
    mockInvitations()
    mockMutations()

    renderPage()

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error state', () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockInvitations({ isError: true, isPending: false, data: undefined })
    mockMutations()

    renderPage()

    expect(screen.getByText('Something went wrong loading your invitations.')).toBeInTheDocument()
  })

  it('shows an empty state when there are no open invitations', () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockInvitations({ data: { pages: [{ data: [], cursor: null, count: 0 }] } })
    mockMutations()

    renderPage()

    expect(screen.getByText('No Invitations Found!')).toBeInTheDocument()
  })

  it('renders each invitation row', () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockInvitations()
    mockMutations()

    renderPage()

    expect(screen.getByText('Oak Leaf Leisure Centre | Organised by @the-organiser')).toBeInTheDocument()
    expect(screen.getByText('The Pitch | Organised by @another-organiser')).toBeInTheDocument()
  })

  it('accepts the clicked row only', async () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockInvitations()
    const { acceptMutate } = mockMutations()
    const user = userEvent.setup()

    renderPage()
    const [firstAccept] = screen.getAllByRole('button', { name: 'Accept' })
    await user.click(firstAccept)

    expect(acceptMutate).toHaveBeenCalledWith('inv-1')
  })

  it('declines the clicked row only', async () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockInvitations()
    const { declineMutate } = mockMutations()
    const user = userEvent.setup()

    renderPage()
    const declineButtons = screen.getAllByRole('button', { name: 'Decline' })
    await user.click(declineButtons[1])

    expect(declineMutate).toHaveBeenCalledWith('inv-2')
  })

  it('toasts the 422 detail message when accept fails', () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockInvitations()
    mockMutations({
      accept: {
        isError: true,
        error: new ApiError(422, { detail: 'Unable to accept: game is already at capacity.' }),
      },
    })

    renderPage()

    expect(toast.error).toHaveBeenCalledWith('Unable to accept: game is already at capacity.')
  })

  it('shows Load More when another page is available', async () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    const fetchNextPage = vi.fn()
    mockInvitations({ hasNextPage: true, fetchNextPage })
    mockMutations()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Load More…' }))

    expect(fetchNextPage).toHaveBeenCalled()
  })

  it('Back returns to the games list', async () => {
    vi.mocked(useSelf).mockReturnValue({ isPending: false, isError: false, data: { id: 'user-1' } } as any)
    mockInvitations()
    mockMutations()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Back' }))

    expect(screen.getByText('Games list')).toBeInTheDocument()
  })
})
