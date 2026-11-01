import { ApiError } from './auth'

export interface GuestInvitation {
  name: string
  isConfirmed: boolean
  wish: string | null
  rsvpGuestCount: number | null
}

export interface GuestRsvpResult {
  guestCount: number
  isConfirmed: boolean
}

export interface GuestWish {
  id: number
  name: string
  wish: string
}

export interface CreatedGuest {
  id: number
  name: string
  invitationUrl: string
}

export interface GuestListItem {
  id: number
  name: string
  phone: string | null
  isConfirmed: boolean
  rsvpGuestCount: number | null
  wish: string | null
  invitationUrl: string | null
}

export interface CreateGuestInput {
  name: string
  phone?: string
}

export async function getGuests(): Promise<GuestListItem[]> {
  const response = await fetch('/api/guests', {
    credentials: 'include',
  })

  const responseText = await response.text()
  let payload: unknown
  if (responseText) {
    try {
      payload = JSON.parse(responseText) as unknown
    } catch {
      payload = responseText
    }
  }

  if (!response.ok) {
    const body =
      typeof payload === 'object' && payload !== null
        ? payload
        : undefined
    const message =
      body && 'detail' in body && typeof body.detail === 'string'
        ? body.detail
        : body && 'title' in body && typeof body.title === 'string'
          ? body.title
          : response.status === 401
            ? 'Your session has expired. Please sign in again.'
            : response.status === 403
              ? 'Your account is not allowed to view guest invitations.'
              : `The API could not load guests (HTTP ${response.status}${response.statusText ? ` ${response.statusText}` : ''}).`

    throw new ApiError(message, response.status)
  }

  if (!Array.isArray(payload)) {
    throw new ApiError('The server returned an unexpected guest list.', 502)
  }

  return payload.map((item): GuestListItem => {
    if (
      typeof item !== 'object' ||
      item === null ||
      !('id' in item) ||
      typeof item.id !== 'number' ||
      !('name' in item) ||
      typeof item.name !== 'string' ||
      !('phone' in item) ||
      (typeof item.phone !== 'string' && item.phone !== null) ||
      !('isConfirmed' in item) ||
      typeof item.isConfirmed !== 'boolean' ||
      !('rsvpGuestCount' in item) ||
      (typeof item.rsvpGuestCount !== 'number' && item.rsvpGuestCount !== null) ||
      !('wish' in item) ||
      (typeof item.wish !== 'string' && item.wish !== null) ||
      !('invitationUrl' in item) ||
      (typeof item.invitationUrl !== 'string' && item.invitationUrl !== null)
    ) {
      throw new ApiError('The server returned an unexpected guest list item.', 502)
    }

    return {
      id: item.id,
      name: item.name,
      phone: item.phone,
      isConfirmed: item.isConfirmed,
      rsvpGuestCount: item.rsvpGuestCount,
      wish: item.wish,
      invitationUrl: item.invitationUrl,
    }
  })
}

export async function createGuest(input: CreateGuestInput): Promise<CreatedGuest> {
  const response = await fetch('/api/guests', {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  })

  const responseText = await response.text()
  let payload: unknown
  if (responseText) {
    try {
      payload = JSON.parse(responseText) as unknown
    } catch {
      payload = responseText
    }
  }

  if (!response.ok) {
    const body =
      typeof payload === 'object' && payload !== null
        ? payload
        : undefined
    const validationErrors =
      body && 'errors' in body && typeof body.errors === 'object' && body.errors !== null
        ? Object.values(body.errors)
            .flatMap((value) => (Array.isArray(value) ? value : [value]))
            .filter((value): value is string => typeof value === 'string')
            .join(' ')
        : ''
    const message =
      body && 'detail' in body && typeof body.detail === 'string'
        ? body.detail
        : body && 'title' in body && typeof body.title === 'string'
          ? validationErrors
            ? `${body.title} ${validationErrors}`
            : body.title
          : body && 'message' in body && typeof body.message === 'string'
            ? body.message
            : response.status === 401
              ? 'Your session has expired. Please sign in again.'
              : response.status === 403
                ? 'Your account is not allowed to create guest invitations.'
                : `The API could not create the invitation (HTTP ${response.status}${response.statusText ? ` ${response.statusText}` : ''}). Check the API logs for details.`

    throw new ApiError(message, response.status)
  }

  if (
    typeof payload !== 'object' ||
    payload === null ||
    !('id' in payload) ||
    typeof payload.id !== 'number' ||
    !('name' in payload) ||
    typeof payload.name !== 'string' ||
    !('invitationUrl' in payload) ||
    typeof payload.invitationUrl !== 'string'
  ) {
    throw new ApiError('The server returned an unexpected guest response.', 502)
  }

  return {
    id: payload.id,
    name: payload.name,
    invitationUrl: payload.invitationUrl,
  }
}

export async function deleteGuest(id: number): Promise<void> {
  const response = await fetch(`/api/guests/${id}`, {
    method: 'DELETE',
    credentials: 'include',
  })

  if (response.ok) return

  const responseText = await response.text()
  let payload: unknown
  if (responseText) {
    try {
      payload = JSON.parse(responseText) as unknown
    } catch {
      payload = responseText
    }
  }

  const body =
    typeof payload === 'object' && payload !== null
      ? payload
      : undefined
  const message =
    body && 'detail' in body && typeof body.detail === 'string'
      ? body.detail
      : body && 'title' in body && typeof body.title === 'string'
        ? body.title
        : response.status === 401
          ? 'Your session has expired. Please sign in again.'
          : response.status === 403
            ? 'Your account is not allowed to delete guests.'
            : response.status === 404
              ? 'This guest was not found. Refresh the guest list and try again.'
              : `The API could not delete the guest (HTTP ${response.status}${response.statusText ? ` ${response.statusText}` : ''}).`

  throw new ApiError(message, response.status)
}

export async function getGuestInvitation(token: string): Promise<GuestInvitation> {
  const response = await fetch(`/api/guests/${encodeURIComponent(token)}`)
  if (!response.ok) {
    throw new Error(
      response.status === 404
        ? 'This invitation link is invalid or has expired.'
        : 'Unable to load this invitation. Please try again later.',
    )
  }

  const payload: unknown = await response.json()
  if (
    typeof payload !== 'object' ||
    payload === null ||
    !('name' in payload) ||
    typeof payload.name !== 'string' ||
    !('isConfirmed' in payload) ||
    typeof payload.isConfirmed !== 'boolean' ||
    !('wish' in payload) ||
    (typeof payload.wish !== 'string' && payload.wish !== null) ||
    !('rsvpGuestCount' in payload) ||
    (typeof payload.rsvpGuestCount !== 'number' && payload.rsvpGuestCount !== null)
  ) {
    throw new Error('The server returned an unexpected invitation response.')
  }

  return {
    name: payload.name,
    isConfirmed: payload.isConfirmed,
    wish: payload.wish,
    rsvpGuestCount: payload.rsvpGuestCount,
  }
}

export async function saveGuestRsvp(
  token: string,
  guestCount: number,
): Promise<GuestRsvpResult> {
  const response = await fetch(`/api/guests/${encodeURIComponent(token)}/rsvp`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ guestCount }),
  })

  const responseText = await response.text()
  let payload: unknown
  if (responseText) {
    try {
      payload = JSON.parse(responseText) as unknown
    } catch {
      payload = responseText
    }
  }

  if (!response.ok) {
    const body = typeof payload === 'object' && payload !== null ? payload : undefined
    const message =
      body && 'message' in body && typeof body.message === 'string'
        ? body.message
        : body && 'detail' in body && typeof body.detail === 'string'
          ? body.detail
          : response.status === 404
            ? 'This invitation link is invalid or has expired.'
            : `Unable to save your RSVP (HTTP ${response.status}${response.statusText ? ` ${response.statusText}` : ''}).`
    throw new Error(message)
  }

  if (
    typeof payload !== 'object' ||
    payload === null ||
    !('guestCount' in payload) ||
    typeof payload.guestCount !== 'number' ||
    !('isConfirmed' in payload) ||
    typeof payload.isConfirmed !== 'boolean'
  ) {
    throw new Error('The server returned an unexpected RSVP response.')
  }

  return {
    guestCount: payload.guestCount,
    isConfirmed: payload.isConfirmed,
  }
}

export async function saveGuestWish(token: string, wish: string): Promise<string> {
  const response = await fetch(`/api/guests/${encodeURIComponent(token)}/wish`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ wish }),
  })

  const responseText = await response.text()
  let payload: unknown
  if (responseText) {
    try {
      payload = JSON.parse(responseText) as unknown
    } catch {
      payload = responseText
    }
  }

  if (!response.ok) {
    const body = typeof payload === 'object' && payload !== null ? payload : undefined
    const message =
      body && 'message' in body && typeof body.message === 'string'
        ? body.message
        : body && 'detail' in body && typeof body.detail === 'string'
          ? body.detail
          : response.status === 404
            ? 'This invitation link is invalid or has expired.'
            : `Unable to save your wish (HTTP ${response.status}${response.statusText ? ` ${response.statusText}` : ''}).`
    throw new Error(message)
  }

  if (
    typeof payload !== 'object' ||
    payload === null ||
    !('wish' in payload) ||
    typeof payload.wish !== 'string'
  ) {
    throw new Error('The server returned an unexpected wish response.')
  }

  return payload.wish
}

export async function getGuestWishes(): Promise<GuestWish[]> {
  const response = await fetch('/api/guests/wishes')
  if (!response.ok) {
    throw new Error(
      `Unable to load guest wishes (HTTP ${response.status}${response.statusText ? ` ${response.statusText}` : ''}).`,
    )
  }

  const payload: unknown = await response.json()
  if (!Array.isArray(payload)) {
    throw new Error('The server returned an unexpected guest wishes response.')
  }

  return payload.map((item): GuestWish => {
    if (
      typeof item !== 'object' ||
      item === null ||
      !('id' in item) ||
      typeof item.id !== 'number' ||
      !('name' in item) ||
      typeof item.name !== 'string' ||
      !('wish' in item) ||
      typeof item.wish !== 'string'
    ) {
      throw new Error('The server returned an unexpected guest wish.')
    }

    return {
      id: item.id,
      name: item.name,
      wish: item.wish,
    }
  })
}
