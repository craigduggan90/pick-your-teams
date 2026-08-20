import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Footer } from './Footer'

describe('Footer', () => {
  it('renders the app name', () => {
    render(<Footer />)
    expect(screen.getByText('Pick Your Teams')).toBeInTheDocument()
  })

  it('renders no actions bar when actions is not given', () => {
    render(<Footer />)
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('renders the given actions instead of the app name', () => {
    render(<Footer actions={<button type="button">Search</button>} />)

    expect(screen.getByRole('button', { name: 'Search' })).toBeInTheDocument()
    expect(screen.queryByText('Pick Your Teams')).not.toBeInTheDocument()
  })
})
