export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  /** Present on 422/400 validation failures — field name to messages for that field. */
  errors?: Record<string, string[]>
  /** Present on 404s. */
  resource?: string
  identifier?: string
}
