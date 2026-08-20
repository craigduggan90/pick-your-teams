import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { GameStatusBadge } from './GameStatusBadge'

describe('GameStatusBadge', () => {
  it('renders Scheduled', () => {
    render(<GameStatusBadge status="Scheduled" />)
    expect(screen.getByText('Scheduled')).toBeInTheDocument()
  })

  it('renders Finished', () => {
    render(<GameStatusBadge status="Finished" />)
    expect(screen.getByText('Finished')).toBeInTheDocument()
  })
})
