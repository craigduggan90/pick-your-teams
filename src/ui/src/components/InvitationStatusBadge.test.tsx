import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { InvitationStatusBadge } from './InvitationStatusBadge'

describe('InvitationStatusBadge', () => {
  it('renders Open as Pending', () => {
    render(<InvitationStatusBadge status="Open" />)
    expect(screen.getByText('Pending')).toBeInTheDocument()
  })

  it('renders Accepted', () => {
    render(<InvitationStatusBadge status="Accepted" />)
    expect(screen.getByText('Accepted')).toBeInTheDocument()
  })

  it('renders Declined', () => {
    render(<InvitationStatusBadge status="Declined" />)
    expect(screen.getByText('Declined')).toBeInTheDocument()
  })

  it('renders Failed', () => {
    render(<InvitationStatusBadge status="Failed" />)
    expect(screen.getByText('Failed')).toBeInTheDocument()
  })
})
