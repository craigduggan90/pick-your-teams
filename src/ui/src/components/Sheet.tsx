import type { ReactNode } from 'react'
import { Dialog as DialogPrimitive } from '@base-ui/react/dialog'
import { XIcon } from 'lucide-react'

export interface SheetProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  description?: string
  children?: ReactNode
  footer?: ReactNode
}

// Drop-in replacement for Modal with the same prop shape, but rendered as a bottom sheet (the
// "Game Details" expando pattern from GameDetailsSheet) instead of a centered dialog. Built
// directly on base-ui's Dialog primitives - same as GameDetailsSheet's look, but with the focus
// trap/Escape-key/aria-hidden-background handling that hand-rolled component didn't have, which
// matters more here since these are destructive confirmations and a real form, not read-only info.
export function Sheet({ open, onOpenChange, title, description, children, footer }: SheetProps) {
  return (
    <DialogPrimitive.Root open={open} onOpenChange={onOpenChange}>
      <DialogPrimitive.Portal>
        <DialogPrimitive.Backdrop className="fixed inset-0 z-50 bg-black/30 data-open:animate-in data-open:fade-in-0 data-closed:animate-out data-closed:fade-out-0" />
        <DialogPrimitive.Popup className="fixed inset-x-0 bottom-0 z-50 mx-auto flex w-full max-w-md flex-col gap-3 rounded-t-xl border-t border-border bg-background p-4 shadow-lg outline-none data-open:animate-in data-open:slide-in-from-bottom data-closed:animate-out data-closed:slide-out-to-bottom">
          <div className="flex items-center justify-between gap-2">
            <div className="flex flex-col gap-1">
              <DialogPrimitive.Title className="font-semibold text-dark-grey">
                {title}
              </DialogPrimitive.Title>
              {description && (
                <DialogPrimitive.Description className="text-sm text-light-grey">
                  {description}
                </DialogPrimitive.Description>
              )}
            </div>
            <DialogPrimitive.Close
              aria-label="Close"
              className="cursor-pointer text-light-grey"
              render={<button type="button" />}
            >
              <XIcon className="size-5" />
            </DialogPrimitive.Close>
          </div>

          {children}

          {footer && (
            <div className="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">{footer}</div>
          )}
        </DialogPrimitive.Popup>
      </DialogPrimitive.Portal>
    </DialogPrimitive.Root>
  )
}
