import { Toaster as BaseToaster } from '@/components/ui/sonner'
import type { ToasterProps } from 'sonner'

export { toast } from 'sonner'

export function Toaster(props: ToasterProps) {
  return <BaseToaster richColors closeButton {...props} />
}
