import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { PageTitleProvider, useHeaderTitle } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { GamesSearchForm } from './GamesSearchForm'

function HeaderTitleStub() {
  return <h1>{useHeaderTitle()}</h1>
}

function FooterActionsStub() {
  return <>{useFooterActions()}</>
}

function renderForm({ onApply = vi.fn(), onCancel = vi.fn() } = {}) {
  render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <PageActionsProvider>
        <HeaderTitleStub />
        <GamesSearchForm filters={{}} onApply={onApply} onCancel={onCancel} />
        <FooterActionsStub />
      </PageActionsProvider>
    </PageTitleProvider>,
  )
  return { onApply, onCancel }
}

describe('GamesSearchForm', () => {
  it('sets the header title to Games / Search', () => {
    renderForm()
    expect(screen.getByRole('heading')).toHaveTextContent('Games / Search')
  })

  it('calls onCancel via the footer Cancel button', async () => {
    const user = userEvent.setup()
    const { onCancel } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(onCancel).toHaveBeenCalled()
  })

  it('applies the selected status filter', async () => {
    const user = userEvent.setup()
    const { onApply } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Scheduled' }))
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(
      expect.objectContaining({ status: 'Scheduled', startTimeFrom: undefined, startTimeTo: undefined }),
    )
  })

  it('deselects a status filter when clicked again', async () => {
    const user = userEvent.setup()
    const { onApply } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Complete' }))
    await user.click(screen.getByRole('button', { name: 'Complete' }))
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ status: undefined }))
  })

  it('includes team size only when a value is entered', async () => {
    const user = userEvent.setup()
    const { onApply } = renderForm()

    await user.type(screen.getByLabelText('Players per Team'), '5')
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ teamSize: 5 }))
  })

  it('only includes the start-from date when its checkbox is enabled', async () => {
    const user = userEvent.setup()
    const { onApply } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Apply' }))
    expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ startTimeFrom: undefined }))

    await user.click(screen.getByLabelText('Game Start From'))
    await user.click(screen.getByRole('button', { name: 'Apply' }))
    expect(onApply).toHaveBeenLastCalledWith(
      expect.objectContaining({ startTimeFrom: expect.any(String) }),
    )
  })
})
