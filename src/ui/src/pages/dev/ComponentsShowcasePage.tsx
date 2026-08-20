import { useState } from 'react'
import { Button } from '@/components/Button'
import { TextInput } from '@/components/TextInput'
import { Modal } from '@/components/Modal'
import { SelectField } from '@/components/Select'
import { toast } from '@/components/Toast'
import { usePageTitle } from '@/hooks/usePageTitle'

const TEAM_OPTIONS = [
  { value: 'home', label: 'To Home Team' },
  { value: 'away', label: 'To Away Team' },
  { value: 'none', label: 'Remove from Team' },
  { value: 'game', label: 'Remove from Game', destructive: true },
]

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="flex flex-col gap-3 border-b border-border pb-8">
      <h2 className="text-sm font-medium text-dark-grey uppercase tracking-wide">{title}</h2>
      {children}
    </section>
  )
}

export function ComponentsShowcasePage() {
  usePageTitle('Component Showcase')
  const [name, setName] = useState('')
  const [tag, setTag] = useState('taken-tag')
  const [playersPerTeam, setPlayersPerTeam] = useState('7')
  const [startTime, setStartTime] = useState('')
  const [team, setTeam] = useState<string>('home')
  const [modalOpen, setModalOpen] = useState(false)

  return (
    <div className="mx-auto flex w-full max-w-md flex-col gap-8 p-4">
      <p className="text-sm text-light-grey">
        Stage 1 primitive showcase — kept permanently as a manual visual reference for later
        stages, not part of the real app flow.
      </p>

      <Section title="Buttons">
        <div className="flex flex-wrap gap-2">
          <Button variant="primary">Primary</Button>
          <Button variant="secondary">Secondary</Button>
          <Button variant="outline">Outline</Button>
          <Button variant="ghost">Ghost</Button>
          <Button variant="destructive">Destructive</Button>
          <Button variant="link">Link</Button>
          <Button variant="primary" disabled>
            Disabled
          </Button>
        </div>
      </Section>

      <Section title="Text input">
        <TextInput label="Display Name" value={name} onChange={(e) => setName(e.target.value)} />
        <TextInput
          label="Tag"
          value={tag}
          onChange={(e) => setTag(e.target.value)}
          error={`'${tag}' is not a valid tag.`}
        />
        <TextInput
          label="Players per Team"
          type="number"
          min={1}
          value={playersPerTeam}
          onChange={(e) => setPlayersPerTeam(e.target.value)}
        />
        <TextInput
          label="Start Time"
          type="datetime-local"
          value={startTime}
          onChange={(e) => setStartTime(e.target.value)}
        />
      </Section>

      <Section title="Select">
        <SelectField
          label="Team"
          value={team}
          onValueChange={setTeam}
          options={TEAM_OPTIONS}
        />
      </Section>

      <Section title="Modal">
        <Button variant="destructive" onClick={() => setModalOpen(true)}>
          Remove @bob?
        </Button>
        <Modal
          open={modalOpen}
          onOpenChange={setModalOpen}
          title="Remove @bob?"
          description="@bob will need a new invite to re-join the game. Are you sure?"
          footer={
            <>
              <Button variant="outline" onClick={() => setModalOpen(false)}>
                Cancel
              </Button>
              <Button variant="destructive" onClick={() => setModalOpen(false)}>
                Remove
              </Button>
            </>
          }
        />
      </Section>

      <Section title="Toast">
        <div className="flex flex-wrap gap-2">
          <Button variant="primary" onClick={() => toast.success('Changes saved!')}>
            Success
          </Button>
          <Button
            variant="destructive"
            onClick={() => toast.error("'taken' is not a valid tag.")}
          >
            Error
          </Button>
          <Button variant="outline" onClick={() => toast.warning('Field error (if we have it)')}>
            Warning
          </Button>
          <Button variant="outline" onClick={() => toast.info('Invitation Accepted/Declined')}>
            Info
          </Button>
        </div>
      </Section>
    </div>
  )
}
