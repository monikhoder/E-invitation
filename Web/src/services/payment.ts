import { ApiError } from './auth'

export interface PaymentSettings {
  abaPaymentEnabled: boolean
  abaUrl: string | null
  qrImageUrl: string
}

export interface AdminPaymentSettings extends PaymentSettings {
  hasUploadedQr: boolean
}

async function readSettings(response: Response): Promise<unknown> {
  const text = await response.text()
  if (!text) return undefined

  try {
    return JSON.parse(text) as unknown
  } catch {
    return text
  }
}

function expectSettings(payload: unknown): PaymentSettings {
  if (
    typeof payload !== 'object' ||
    payload === null ||
    !('abaPaymentEnabled' in payload) ||
    typeof payload.abaPaymentEnabled !== 'boolean' ||
    !('abaUrl' in payload) ||
    (typeof payload.abaUrl !== 'string' && payload.abaUrl !== null) ||
    !('qrImageUrl' in payload) ||
    typeof payload.qrImageUrl !== 'string'
  ) {
    throw new ApiError('The server returned unexpected payment settings.', 502)
  }

  return {
    abaPaymentEnabled: payload.abaPaymentEnabled,
    abaUrl: payload.abaUrl,
    qrImageUrl: payload.qrImageUrl,
  }
}

function getErrorMessage(payload: unknown, status: number, statusText: string): string {
  if (typeof payload === 'object' && payload !== null) {
    if ('message' in payload && typeof payload.message === 'string') return payload.message
    if ('detail' in payload && typeof payload.detail === 'string') return payload.detail
    if ('title' in payload && typeof payload.title === 'string') return payload.title
  }

  if (status === 401) return 'Your session has expired. Please sign in again.'
  if (status === 403) return 'Your account is not allowed to update payment settings.'
  return `The payment settings request failed (HTTP ${status}${statusText ? ` ${statusText}` : ''}).`
}

export async function getPublicPaymentSettings(): Promise<PaymentSettings> {
  const response = await fetch('/api/payment-settings')
  const payload = await readSettings(response)
  if (!response.ok) {
    throw new Error(getErrorMessage(payload, response.status, response.statusText))
  }

  return expectSettings(payload)
}

export async function getAdminPaymentSettings(): Promise<AdminPaymentSettings> {
  const response = await fetch('/api/payment-settings/admin', {
    credentials: 'include',
  })
  const payload = await readSettings(response)
  if (!response.ok) {
    throw new ApiError(
      getErrorMessage(payload, response.status, response.statusText),
      response.status,
    )
  }

  const settings = expectSettings(payload)
  if (
    typeof payload !== 'object' ||
    payload === null ||
    !('hasUploadedQr' in payload) ||
    typeof payload.hasUploadedQr !== 'boolean'
  ) {
    throw new ApiError('The server returned unexpected admin payment settings.', 502)
  }

  return {
    ...settings,
    hasUploadedQr: payload.hasUploadedQr,
  }
}

export async function updateAdminPaymentSettings(
  abaPaymentEnabled: boolean,
  abaUrl: string,
  qrImage: File | null,
): Promise<AdminPaymentSettings> {
  const formData = new FormData()
  formData.set('AbaPaymentEnabled', String(abaPaymentEnabled))
  formData.set('AbaUrl', abaUrl)
  if (qrImage) formData.set('QrImage', qrImage)

  const response = await fetch('/api/payment-settings', {
    method: 'PUT',
    credentials: 'include',
    body: formData,
  })
  const payload = await readSettings(response)
  if (!response.ok) {
    throw new ApiError(
      getErrorMessage(payload, response.status, response.statusText),
      response.status,
    )
  }

  return getAdminPaymentSettings()
}
