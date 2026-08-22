import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { InvitationListItem } from './InvitationListItem'
import type { InvitationModel } from '@/api/invitations'

const baseInvitation: InvitationModel = {
  id: 'inv-1',
  status: 'Open',
  game: {
    id: 'game-1',
    startTime: '2026-08-10T20:00:00.000Z',
    duration: 60,
    location: 'Oak Leaf Leisure Centre',
  },
  organiser: { id: 'user-1', tag: 'little-bobby-tables', displayName: 'Robert D. Tables' },
}

describe('InvitationListItem', () => {
  it('renders the formatted date, location, and organiser', () => {
    render(
      <InvitationListItem
        invitation={baseInvitation}
        onAccept={vi.fn()}
        onDecline={vi.fn()}
        isAccepting={false}
        isDeclining={false}
      />,
    )

    expect(screen.getByText('Monday 10th August @ 20:00 (UTC)')).toBeInTheDocument()
    expect(screen.getByText('Oak Leaf Leisure Centre | Organised by @little-bobby-tables')).toBeInTheDocument()
  })

  it('falls back to placeholder location text and omits the organiser when null', () => {
    render(
      <InvitationListItem
        invitation={{ ...baseInvitation, game: { ...baseInvitation.game, location: null }, organiser: null }}
        onAccept={vi.fn()}
        onDecline={vi.fn()}
        isAccepting={false}
        isDeclining={false}
      />,
    )

    expect(screen.getByText('Location TBC')).toBeInTheDocument()
  })

  it('calls onAccept when Accept is clicked', async () => {
    const onAccept = vi.fn()
    const user = userEvent.setup()
    render(
      <InvitationListItem
        invitation={baseInvitation}
        onAccept={onAccept}
        onDecline={vi.fn()}
        isAccepting={false}
        isDeclining={false}
      />,
    )

    await user.click(screen.getByRole('button', { name: 'Accept' }))

    expect(onAccept).toHaveBeenCalled()
  })

  it('calls onDecline when Decline is clicked', async () => {
    const onDecline = vi.fn()
    const user = userEvent.setup()
    render(
      <InvitationListItem
        invitation={baseInvitation}
        onAccept={vi.fn()}
        onDecline={onDecline}
        isAccepting={false}
        isDeclining={false}
      />,
    )

    await user.click(screen.getByRole('button', { name: 'Decline' }))

    expect(onDecline).toHaveBeenCalled()
  })

  it('disables both buttons while accepting', () => {
    render(
      <InvitationListItem
        invitation={baseInvitation}
        onAccept={vi.fn()}
        onDecline={vi.fn()}
        isAccepting={true}
        isDeclining={false}
      />,
    )

    expect(screen.getByRole('button', { name: 'Accept' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Decline' })).toBeDisabled()
  })

  it('disables both buttons while declining', () => {
    render(
      <InvitationListItem
        invitation={baseInvitation}
        onAccept={vi.fn()}
        onDecline={vi.fn()}
        isAccepting={false}
        isDeclining={true}
      />,
    )

    expect(screen.getByRole('button', { name: 'Accept' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Decline' })).toBeDisabled()
  })
})
