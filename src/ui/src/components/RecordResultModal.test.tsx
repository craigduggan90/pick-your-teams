import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { RecordResultModal } from './RecordResultModal'

describe('RecordResultModal', () => {
  it('disables Confirm until an option is selected', async () => {
    const user = userEvent.setup()
    render(
      <RecordResultModal open onOpenChange={vi.fn()} onConfirm={vi.fn()} isPending={false} />,
    )

    expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled()

    await user.click(screen.getByRole('button', { name: 'Home Team' }))
    expect(screen.getByRole('button', { name: 'Confirm' })).toBeEnabled()
  })

  it('calls onConfirm with the selected winner, including Draw as None', async () => {
    const onConfirm = vi.fn()
    const user = userEvent.setup()
    render(
      <RecordResultModal open onOpenChange={vi.fn()} onConfirm={onConfirm} isPending={false} />,
    )

    await user.click(screen.getByRole('button', { name: 'Draw' }))
    await user.click(screen.getByRole('button', { name: 'Confirm' }))

    expect(onConfirm).toHaveBeenCalledWith('None')
  })

  it('shows a saving state and disables the options while pending', () => {
    render(<RecordResultModal open onOpenChange={vi.fn()} onConfirm={vi.fn()} isPending />)

    expect(screen.getByRole('button', { name: 'Saving…' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Home Team' })).toBeDisabled()
  })

  it('calls onOpenChange(false) on Cancel', async () => {
    const onOpenChange = vi.fn()
    const user = userEvent.setup()
    render(
      <RecordResultModal open onOpenChange={onOpenChange} onConfirm={vi.fn()} isPending={false} />,
    )

    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(onOpenChange).toHaveBeenCalledWith(false)
  })
})
