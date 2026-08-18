import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Header } from './Header'

describe('Header', () => {
  it('renders the screen title', () => {
    render(<Header title="Games" />)
    expect(screen.getByRole('heading', { name: 'Games' })).toBeInTheDocument()
  })

  it('renders a custom account slot when provided', () => {
    render(<Header title="Games" accountSlot={<button>Account</button>} />)
    expect(screen.getByRole('button', { name: 'Account' })).toBeInTheDocument()
  })
})
