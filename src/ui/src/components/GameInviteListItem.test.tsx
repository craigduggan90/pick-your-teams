import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { GameInviteListItem } from './GameInviteListItem'
import type { InvitationModel } from '@/api/invitations'

const baseInvitation: InvitationModel = {
  id: 'inv-1',
  status: 'Accepted',
  game: {
    id: 'game-1',
    startTime: '2026-08-10T20:00:00.000Z',
    duration: 60,
    location: 'Oak Leaf Leisure Centre',
  },
  organiser: { id: 'organiser-1', tag: 'the-organiser', displayName: 'The Organiser' },
  invitee: { id: 'user-2', tag: 'monkey-duster', displayName: 'Jordan Monk' },
}

describe('GameInviteListItem', () => {
  it('renders the invitee display name, tag, and status', () => {
    render(<GameInviteListItem invitation={baseInvitation} />)

    expect(screen.getByText('Jordan Monk (@monkey-duster)')).toBeInTheDocument()
    expect(screen.getByText('Accepted')).toBeInTheDocument()
  })

  it('falls back to a placeholder when invitee is null', () => {
    render(<GameInviteListItem invitation={{ ...baseInvitation, invitee: null }} />)

    expect(screen.getByText('Unknown user')).toBeInTheDocument()
  })
})
