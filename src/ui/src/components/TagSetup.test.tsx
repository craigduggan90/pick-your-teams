import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useUpdateTag } from '@/hooks/useUpdateTag'
import { ApiError } from '@/api/client'
import { toast } from '@/components/Toast'
import { TagSetup } from './TagSetup'

vi.mock('@/hooks/useUpdateTag')
vi.mock('@/components/Toast', () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}))

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

  it('shows a saving state on the button and disables the field while pending', () => {
    mockMutation({ isPending: true })
    render(<TagSetup mode="gate" userId="user-1" />)

    expect(screen.getByRole('button', { name: 'Saving…' })).toBeDisabled()
    expect(screen.getByLabelText('Tag')).toBeDisabled()
  })

  it('shows the field error and toasts the reason on a validation failure', () => {
    const error = new ApiError(422, {
      title: 'Validation Error',
      detail: 'One or more validation failures occurred.',
      errors: { Tag: ['Tag not available.'] },
    })
    mockMutation({ isError: true, error })
    render(<TagSetup mode="gate" userId="user-1" />)

    expect(screen.getByText('Tag not available.')).toBeInTheDocument()
    expect(toast.error).toHaveBeenCalledWith('One or more validation failures occurred.')
  })

  it('toasts a generic message on a non-API failure', () => {
    // getAccessTokenSilently (or a raw network failure) can reject with a plain Error even
    // though the mutation's declared error type is ApiError — this exercises that fallback.
    mockMutation({ isError: true, error: new Error('network down') as unknown as ApiError })
    render(<TagSetup mode="gate" userId="user-1" />)

    expect(toast.error).toHaveBeenCalledWith('Something went wrong saving your tag.')
  })

  it('toasts success and calls onSuccess immediately', () => {
    mockMutation({ isSuccess: true })
    const onSuccess = vi.fn()
    render(<TagSetup mode="gate" userId="user-1" onSuccess={onSuccess} />)

    expect(toast.success).toHaveBeenCalledWith('Tag saved!')
    expect(screen.getByLabelText('Tag')).toBeDisabled()
    expect(onSuccess).toHaveBeenCalled()
  })
})
