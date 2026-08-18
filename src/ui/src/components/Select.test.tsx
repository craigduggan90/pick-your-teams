import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SelectField } from './Select'

const TEAM_OPTIONS = [
  { value: 'away', label: 'To Away Team' },
  { value: 'none', label: 'Remove from Team' },
  { value: 'game', label: 'Remove from Game', destructive: true },
]

describe('SelectField', () => {
  it('renders the label and placeholder', () => {
    render(
      <SelectField label="Team" placeholder="Choose a team" options={TEAM_OPTIONS} />,
    )

    expect(screen.getByText('Team')).toBeInTheDocument()
    expect(screen.getByText('Choose a team')).toBeInTheDocument()
  })

  it('opens and calls onValueChange when an option is selected', async () => {
    const user = userEvent.setup()
    const onValueChange = vi.fn()
    render(
      <SelectField
        label="Team"
        options={TEAM_OPTIONS}
        onValueChange={onValueChange}
      />,
    )

    await user.click(screen.getByRole('combobox'))
    await user.click(await screen.findByRole('option', { name: 'To Away Team' }))

    expect(onValueChange).toHaveBeenCalledWith('away')
  })

  it('shows the currently selected option', () => {
    render(
      <SelectField label="Team" value="none" options={TEAM_OPTIONS} />,
    )

    expect(screen.getByText('Remove from Team')).toBeInTheDocument()
  })
})
