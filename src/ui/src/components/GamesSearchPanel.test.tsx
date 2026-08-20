import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { GamesSearchPanel } from './GamesSearchPanel'

describe('GamesSearchPanel', () => {
  it('renders nothing when closed', () => {
    render(<GamesSearchPanel open={false} onOpenChange={vi.fn()} filters={{}} onApply={vi.fn()} />)

    expect(screen.queryByText('Games / Search')).not.toBeInTheDocument()
  })

  it('closes without applying on Cancel', async () => {
    const onOpenChange = vi.fn()
    const onApply = vi.fn()
    const user = userEvent.setup()
    render(<GamesSearchPanel open onOpenChange={onOpenChange} filters={{}} onApply={onApply} />)

    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(onOpenChange).toHaveBeenCalledWith(false)
    expect(onApply).not.toHaveBeenCalled()
  })

  it('applies the selected status filter and closes', async () => {
    const onOpenChange = vi.fn()
    const onApply = vi.fn()
    const user = userEvent.setup()
    render(<GamesSearchPanel open onOpenChange={onOpenChange} filters={{}} onApply={onApply} />)

    await user.click(screen.getByRole('button', { name: 'Scheduled' }))
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(
      expect.objectContaining({ status: 'Scheduled', startTimeFrom: undefined, startTimeTo: undefined }),
    )
    expect(onOpenChange).toHaveBeenCalledWith(false)
  })

  it('deselects a status filter when clicked again', async () => {
    const onApply = vi.fn()
    const user = userEvent.setup()
    render(<GamesSearchPanel open onOpenChange={vi.fn()} filters={{}} onApply={onApply} />)

    await user.click(screen.getByRole('button', { name: 'Complete' }))
    await user.click(screen.getByRole('button', { name: 'Complete' }))
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ status: undefined }))
  })

  it('includes team size only when a value is entered', async () => {
    const onApply = vi.fn()
    const user = userEvent.setup()
    render(<GamesSearchPanel open onOpenChange={vi.fn()} filters={{}} onApply={onApply} />)

    await user.type(screen.getByLabelText('Players per Team'), '5')
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ teamSize: 5 }))
  })

  it('only includes the start-from date when its checkbox is enabled', async () => {
    const onApply = vi.fn()
    const user = userEvent.setup()
    render(<GamesSearchPanel open onOpenChange={vi.fn()} filters={{}} onApply={onApply} />)

    await user.click(screen.getByRole('button', { name: 'Apply' }))
    expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ startTimeFrom: undefined }))

    await user.click(screen.getByLabelText('Game Start From'))
    await user.click(screen.getByRole('button', { name: 'Apply' }))
    expect(onApply).toHaveBeenLastCalledWith(
      expect.objectContaining({ startTimeFrom: expect.any(String) }),
    )
  })
})
