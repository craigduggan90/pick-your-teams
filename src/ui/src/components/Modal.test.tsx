import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Modal } from './Modal'
import { Button } from './Button'

describe('Modal', () => {
  it('renders nothing when closed', () => {
    render(
      <Modal open={false} onOpenChange={() => {}} title="Remove @bob?">
        Body copy
      </Modal>,
    )

    expect(screen.queryByText('Remove @bob?')).not.toBeInTheDocument()
  })

  it('renders the title, description, and body when open', () => {
    render(
      <Modal
        open
        onOpenChange={() => {}}
        title="Remove @bob?"
        description="This cannot be undone."
      >
        <p>Body copy</p>
      </Modal>,
    )

    expect(screen.getByText('Remove @bob?')).toBeInTheDocument()
    expect(screen.getByText('This cannot be undone.')).toBeInTheDocument()
    expect(screen.getByText('Body copy')).toBeInTheDocument()
  })

  it('renders footer actions', () => {
    render(
      <Modal
        open
        onOpenChange={() => {}}
        title="Remove @bob?"
        footer={
          <>
            <Button variant="outline">Cancel</Button>
            <Button variant="destructive">Remove</Button>
          </>
        }
      />,
    )

    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Remove' })).toBeInTheDocument()
  })

  it('calls onOpenChange when the close button is clicked', async () => {
    const user = userEvent.setup()
    const onOpenChange = vi.fn()
    render(
      <Modal open onOpenChange={onOpenChange} title="Remove @bob?">
        Body copy
      </Modal>,
    )

    await user.click(screen.getByRole('button', { name: 'Close' }))

    expect(onOpenChange).toHaveBeenCalled()
    expect(onOpenChange.mock.calls[0][0]).toBe(false)
  })
})
