import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Toaster, toast } from './Toast'

describe('Toast', () => {
  it('shows a success toast', async () => {
    render(<Toaster />)

    toast.success('Changes saved!')

    expect(await screen.findByText('Changes saved!')).toBeInTheDocument()
  })

  it('shows an error toast', async () => {
    render(<Toaster />)

    toast.error("'taken' is not a valid tag.")

    expect(await screen.findByText("'taken' is not a valid tag.")).toBeInTheDocument()
  })
})
