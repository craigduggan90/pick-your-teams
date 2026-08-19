import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Loading } from './Loading'

describe('Loading', () => {
  it('renders a loading message', () => {
    render(<Loading />)
    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })
})
