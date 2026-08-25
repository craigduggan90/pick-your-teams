# Todo

Scoped-but-not-yet-built work. Each entry records the decisions made and alternatives rejected
during scoping, so a future session can go straight to implementation without re-deriving them.
When an entry is built, delete it from here (the PR/commit history is the record of what
shipped, not this file).

## Invite Players: tag suggestions

Scoped 2026-08-25. Splits into two independent efforts — either can ship without the other.

### Rejected direction: explicit "friends"/social graph

Considered pre-populating suggestions from an explicit `Friendship` relationship (request/accept,
like a second `Invitations`-shaped system) rather than deriving from game history. Rejected:
deliberately not building a social feature. Derived-from-history has no cold-start problem either
(a brand new user has zero friends but may already have game history), which was a secondary
reason to prefer deriving.

### Effort 1 — Recent Players quick-invite

A list of players the organiser has recently played *with* (not organised for — scoped via
`Player.UserId == self` → sibling `Player` rows in the same `GameId`, regardless of who organised
that game), rendered at the bottom of the Invite Players form using the same tick/cross
interaction as the My Invitations accept/decline screen.

Behaviour is **instant-send, not add-to-batch**:

- Ticking a row calls `POST /invitations` immediately with a single-tag array (same endpoint the
  main form uses, reused as a batch of one — no new endpoint needed for the send itself).
- Success → toast, remove that tag from the local recent-players list, backfill the vacated slot
  from the next entry in the already-fetched set.
- Failure (already invited, already a player, etc.) → error toast, row stays in place.
- This is deliberately inconsistent with the main form's compose-then-submit flow (see Effort 2)
  — recent players are a high-confidence, low-friction shortcut, not part of the reviewed batch.
  A user could have unsent rows in the main compose list while a Recent Players tick has already
  gone out.

New backend query needed: fetch up to 100 distinct co-played users for the caller, most-recent
game first, **user players only** (`Player.UserId != null` — dummy players have no `Tag` to
invite). Fetched once on page load; the FE list of 100 (not just the 5 shown) lives in local
state so ticks can backfill without refetching. No existing repository method does this join —
`GamesRepository` already proves the `Player`/`Game` join pattern is cheap and index-backed, so
this is a new query method, not a new table/relationship.

Suggested endpoint home: `GET /users/self/recently-played-with` (self-scoped like `GET
/users/self`, not nested under `/games/:id`, since the data isn't game-specific).

**Cache this query.** In-memory for now (we're pre-production), but built against .NET's
`IDistributedCache` so swapping to Redis later is a config change, not a rewrite — use the
built-in `MemoryDistributedCache` (implements `IDistributedCache` over `IMemoryCache`), not a
bespoke in-memory store. Wrap it behind a service so the caller never touches serialization:

```csharp
public interface ICacheClient
{
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory);
    Task ExpireAsync(string key);
}
```

The implementation owns JSON serialize/deserialize to and from the string `IDistributedCache`
stores; the consumer just gets a `T` back. Factory is async (`Func<Task<T>>`, not `Func<T>`)
since the factory here is a database hit.

Note: `Teams.Authoriser` already has an `ICacheClient` (`Teams.Authoriser/Caching`) — but that one
is deliberately plain in-memory, not `IDistributedCache`-backed (see the "Auth model" section of
[claude.md](../claude.md)). This is a **new, separate implementation in `Teams.Api`**, same
interface shape but a different backing store and a different assembly — don't conflate the two
or try to unify them.

Cache key: something like `recently-played-with:{userId}`. Open question for implementation time,
not blocking scope: TTL vs explicit invalidation. The underlying data only changes when a game's
players or result change, so either a modest TTL (15–60 min) or an explicit `ExpireAsync` call at
that point would work — not worth deciding until it's actually built.

### Effort 2 — "All" tag search in the main compose form

Independent of Effort 1. A live search-as-you-type suggestion box for the main form's tag rows,
searching **all users by tag prefix**, not just recent co-players — e.g. typing "dot" returns the
top 5 users whose tag starts with "dot", alphabetically, case-insensitive (matching the existing
case-insensitive tag-lookup convention).

- New endpoint needed: nothing today does prefix search — `GetByTagAsync` is exact-match only.
  Needs something like `GET /users?tagPrefix=dot&take=5`. Response should return tag (+ display
  name) only, not email/other fields.
- Needs a minimum prefix length (3 chars) and debounce before firing, since — unlike Effort 1's
  pre-fetched list — this is a live per-keystroke-adjacent query. Empty state below 3 chars:
  "Enter three letters of their tag to get suggestions".
  Loading state needed too (debounced search may take a moment) — likely a skeleton row matching
  the suggestion row shape, not a spinner, so a fast (<300ms) response doesn't just flash.
- A tick here fills a row in the existing compose list (same as typing manually) — it does
  **not** instant-send like Effort 1. Confirmed: the main form has **no client-side conflict
  checking today** (`InvitePlayersPage.tsx` only trims/dedupes-blank rows before enabling
  Submit — no dedup across rows, no check against existing players/open invitations, no
  self-invite check; everything surfaces post-submit as the existing flat 422 error list). Adding
  suggestions here doesn't need to change that — a suggested tag behaves exactly like a typed one.
- An earlier idea of a "Recent / All" toggle governing a single suggestion box (reusing Effort 1's
  fetched list for the type-ahead too) was superseded by keeping the two efforts fully separate —
  simpler, and avoids a merge/ranking decision between the two sources.
