export interface AdminUser {
  isAuthenticated: true
  username: string
  role: 'Admin'
}

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

interface LoginResponse {
  message: string
  username: string
}

async function request(path: string, init?: RequestInit): Promise<unknown> {
  const headers = new Headers(init?.headers)
  if (init?.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const response = await fetch(`/api/${path}`, {
    ...init,
    credentials: 'include',
    headers,
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
    const message =
      typeof payload === 'object' &&
      payload !== null &&
      'message' in payload &&
      typeof payload.message === 'string'
        ? payload.message
        : response.status === 401
          ? 'Your session has expired. Please sign in again.'
          : 'The request could not be completed.'

    throw new ApiError(message, response.status)
  }

  return payload
}

function expectObject(payload: unknown): Record<string, unknown> {
  if (typeof payload !== 'object' || payload === null || Array.isArray(payload)) {
    throw new ApiError('The server returned an unexpected response.', 502)
  }

  return payload as Record<string, unknown>
}

export async function login(username: string, password: string): Promise<LoginResponse> {
  const payload = expectObject(await request('auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  }))

  if (typeof payload.message !== 'string' || typeof payload.username !== 'string') {
    throw new ApiError('The server returned an unexpected response.', 502)
  }

  return { message: payload.message, username: payload.username }
}

export async function getCurrentAdmin(): Promise<AdminUser> {
  const payload = expectObject(await request('auth/me'))

  if (
    payload.isAuthenticated !== true ||
    typeof payload.username !== 'string' ||
    payload.role !== 'Admin'
  ) {
    throw new ApiError('The server returned an unexpected response.', 502)
  }

  return {
    isAuthenticated: true,
    username: payload.username,
    role: 'Admin',
  }
}

export async function logout(): Promise<{ message: string }> {
  const payload = expectObject(await request('auth/logout', { method: 'POST' }))
  if (typeof payload.message !== 'string') {
    throw new ApiError('The server returned an unexpected response.', 502)
  }

  return { message: payload.message }
}
