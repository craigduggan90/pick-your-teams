import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ErrorMessage } from './ErrorMessage'

describe('ErrorMessage', () => {
  it('renders its message', () => {
    render(<ErrorMessage>Something went wrong.</ErrorMessage>)
    expect(screen.getByText('Something went wrong.')).toBeInTheDocument()
  })
})
