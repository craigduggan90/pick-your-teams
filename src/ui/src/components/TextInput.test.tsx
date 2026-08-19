import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState } from 'react'
import { TextInput } from './TextInput'

function ControlledTextInput(props: { label: string; error?: string }) {
  const [value, setValue] = useState('')
  return (
    <TextInput
      {...props}
      value={value}
      onChange={(event) => setValue(event.target.value)}
    />
  )
}

describe('TextInput', () => {
  it('associates the label with the input', () => {
    render(<TextInput label="Display Name" value="" onChange={() => {}} />)
    expect(screen.getByLabelText('Display Name')).toBeInTheDocument()
  })

  it('floats the label once a value is typed', async () => {
    const user = userEvent.setup()
    render(<ControlledTextInput label="Display Name" />)

    const label = screen.getByText('Display Name')
    const input = screen.getByLabelText('Display Name')

    expect(label).not.toHaveAttribute('data-floated')

    await user.type(input, 'Mike Rotch')

    expect(label).toHaveAttribute('data-floated', 'true')
  })

  it('floats the label while focused even with no value', async () => {
    const user = userEvent.setup()
    render(<TextInput label="Display Name" value="" onChange={() => {}} />)

    const input = screen.getByLabelText('Display Name')
    await user.click(input)

    expect(screen.getByText('Display Name')).toHaveAttribute('data-floated', 'true')
  })

  it('shows an error message and marks the input invalid', () => {
    render(
      <TextInput
        label="Tag"
        value="taken"
        onChange={() => {}}
        error="'taken' is not a valid tag."
      />,
    )

    expect(screen.getByText("'taken' is not a valid tag.")).toBeInTheDocument()
    expect(screen.getByLabelText('Tag')).toHaveAttribute('aria-invalid', 'true')
  })

  it('forwards onChange with the typed value', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<TextInput label="Tag" value="" onChange={onChange} />)

    await user.type(screen.getByLabelText('Tag'), 'a')

    expect(onChange).toHaveBeenCalled()
  })
})
