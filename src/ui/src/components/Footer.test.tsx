import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Footer } from './Footer'

describe('Footer', () => {
  it('renders the app name', () => {
    render(<Footer />)
    expect(screen.getByText('Pick Your Teams')).toBeInTheDocument()
  })
})
