import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { CopyGameModal } from './CopyGameModal'

const sourceStartTime = '2026-08-10T20:00:00.000Z'

describe('CopyGameModal', () => {
  it('pre-fills Start Time to a week after the source game', () => {
    render(
      <CopyGameModal
        open
        onOpenChange={vi.fn()}
        sourceStartTime={sourceStartTime}
        onConfirm={vi.fn()}
        isPending={false}
      />,
    )

    expect(screen.getByLabelText('Start Time')).toHaveValue('2026-08-17T20:00')
  })

  it('calls onConfirm with the current Start Time value', async () => {
    const onConfirm = vi.fn()
    const user = userEvent.setup()
    render(
      <CopyGameModal
        open
        onOpenChange={vi.fn()}
        sourceStartTime={sourceStartTime}
        onConfirm={onConfirm}
        isPending={false}
      />,
    )

    await user.click(screen.getByRole('button', { name: 'Copy Game' }))

    expect(onConfirm).toHaveBeenCalledWith('2026-08-17T20:00')
  })

  it('shows a copying state and disables Cancel/Confirm while pending', () => {
    render(
      <CopyGameModal
        open
        onOpenChange={vi.fn()}
        sourceStartTime={sourceStartTime}
        onConfirm={vi.fn()}
        isPending
      />,
    )

    expect(screen.getByRole('button', { name: 'Copying…' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled()
  })

  it('calls onOpenChange(false) on Cancel', async () => {
    const onOpenChange = vi.fn()
    const user = userEvent.setup()
    render(
      <CopyGameModal
        open
        onOpenChange={onOpenChange}
        sourceStartTime={sourceStartTime}
        onConfirm={vi.fn()}
        isPending={false}
      />,
    )

    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(onOpenChange).toHaveBeenCalledWith(false)
  })
})
