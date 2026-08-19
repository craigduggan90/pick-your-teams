import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useUpdateTag } from '@/hooks/useUpdateTag'
import { ApiError } from '@/api/client'
import { TagSetup } from './TagSetup'

vi.mock('@/hooks/useUpdateTag')

function mockMutation(overrides: Partial<ReturnType<typeof useUpdateTag>> = {}) {
  const mutate = vi.fn()
  vi.mocked(useUpdateTag).mockReturnValue({
    mutate,
    isPending: false,
    isSuccess: false,
    isError: false,
    error: null,
    ...overrides,
  } as unknown as ReturnType<typeof useUpdateTag>)
  return mutate
}

describe('TagSetup', () => {
  it('renders the requirements list and body copy', () => {
    mockMutation()
    render(<TagSetup mode="gate" userId="user-1" />)

    expect(screen.getByText(/main way your friends will find you/)).toBeInTheDocument()
    expect(screen.getByText('3–36 characters')).toBeInTheDocument()
  })

  it('omits the Back button in gate mode', () => {
    mockMutation()
    render(<TagSetup mode="gate" userId="user-1" />)

    expect(screen.queryByRole('button', { name: 'Back' })).not.toBeInTheDocument()
  })

  it('shows a Back button in normal mode and calls onCancel', async () => {
    mockMutation()
    const onCancel = vi.fn()
    const user = userEvent.setup()
    render(<TagSetup mode="normal" userId="user-1" currentTag="bob" onCancel={onCancel} />)

    await user.click(screen.getByRole('button', { name: 'Back' }))

    expect(onCancel).toHaveBeenCalled()
  })

  it('prefills the current tag in normal mode', () => {
    mockMutation()
    render(<TagSetup mode="normal" userId="user-1" currentTag="bob" />)

    expect(screen.getByLabelText('Tag')).toHaveValue('bob')
  })

  it('disables Save until a tag is entered, then calls mutate on click', async () => {
    const mutate = mockMutation()
    const user = userEvent.setup()
    render(<TagSetup mode="gate" userId="user-1" />)

    expect(screen.getByRole('button', { name: 'Save' })).toBeDisabled()

    await user.type(screen.getByLabelText('Tag'), 'new_tag')
    expect(screen.getByRole('button', { name: 'Save' })).toBeEnabled()

    await user.click(screen.getByRole('button', { name: 'Save' }))
    expect(mutate).toHaveBeenCalledWith('new_tag')
  })

  it('shows a saving banner and disables the field while pending', () => {
    mockMutation({ isPending: true })
    render(<TagSetup mode="gate" userId="user-1" />)

    expect(screen.getByText('Saving…')).toBeInTheDocument()
    expect(screen.getByLabelText('Tag')).toBeDisabled()
  })

  it('shows the field error and banner reason on a validation failure', () => {
    const error = new ApiError(422, {
      title: 'Validation Error',
      detail: 'One or more validation failures occurred.',
      errors: { Tag: ['Tag not available.'] },
    })
    mockMutation({ isError: true, error })
    render(<TagSetup mode="gate" userId="user-1" />)

    expect(screen.getByText('Tag not available.')).toBeInTheDocument()
    expect(screen.getByRole('alert')).toHaveTextContent('One or more validation failures occurred.')
  })

  describe('success', () => {
    beforeEach(() => {
      vi.useFakeTimers()
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    it('shows a success banner and calls onSuccess after a short delay', () => {
      mockMutation({ isSuccess: true })
      const onSuccess = vi.fn()
      render(<TagSetup mode="gate" userId="user-1" onSuccess={onSuccess} />)

      expect(screen.getByText('Tag Saved. Redirecting you now!')).toBeInTheDocument()
      expect(screen.getByLabelText('Tag')).toBeDisabled()
      expect(onSuccess).not.toHaveBeenCalled()

      vi.advanceTimersByTime(1200)

      expect(onSuccess).toHaveBeenCalled()
    })
  })
})
