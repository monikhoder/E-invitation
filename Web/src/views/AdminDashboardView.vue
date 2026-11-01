<script setup lang="ts">
import QRCode from 'qrcode'
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError, getCurrentAdmin, logout, type AdminUser } from '../services/auth'
import {
  createGuest,
  deleteGuest,
  getGuests,
  type CreatedGuest,
  type GuestListItem,
} from '../services/guests'
import {
  getAdminPaymentSettings,
  updateAdminPaymentSettings,
} from '../services/payment'

const router = useRouter()
const admin = ref<AdminUser | null>(null)
const errorMessage = ref('')
const isLoading = ref(true)
const isSigningOut = ref(false)
const guestName = ref('')
const guestPhone = ref('')
const guestError = ref('')
const isCreatingGuest = ref(false)
const createdGuest = ref<CreatedGuest | null>(null)
const copiedInvitation = ref(false)
const guests = ref<GuestListItem[]>([])
const isLoadingGuests = ref(false)
const guestListError = ref('')
const guestCopyError = ref('')
const copiedGuestId = ref<number | null>(null)
const deletingGuestId = ref<number | null>(null)
const deleteGuestError = ref('')
const qrDownloadingGuestId = ref<number | null>(null)
const qrDownloadError = ref('')
const abaPaymentEnabled = ref(true)
const paymentAbaUrl = ref('')
const paymentQrUrl = ref('/images/aba%20QR.png')
const paymentQrFile = ref<File | null>(null)
const paymentQrPreviewUrl = ref('')
const paymentQrInput = ref<HTMLInputElement | null>(null)
const hasUploadedPaymentQr = ref(false)
const paymentSettingsError = ref('')
const paymentSettingsMessage = ref('')
const isLoadingPaymentSettings = ref(false)
const isSavingPaymentSettings = ref(false)

onMounted(async () => {
  try {
    admin.value = await getCurrentAdmin()
    await Promise.all([loadGuests(), loadPaymentSettings()])
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      await router.replace({ name: 'admin-login' })
      return
    }

    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to load your administrator account.'
  } finally {
    isLoading.value = false
  }
})

async function loadPaymentSettings() {
  isLoadingPaymentSettings.value = true
  paymentSettingsError.value = ''
  try {
    const settings = await getAdminPaymentSettings()
    abaPaymentEnabled.value = settings.abaPaymentEnabled
    paymentAbaUrl.value = settings.abaUrl ?? ''
    paymentQrUrl.value = settings.qrImageUrl
    hasUploadedPaymentQr.value = settings.hasUploadedQr
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      await router.replace({ name: 'admin-login' })
      return
    }
    paymentSettingsError.value =
      error instanceof Error ? error.message : 'Unable to load payment settings.'
  } finally {
    isLoadingPaymentSettings.value = false
  }
}

function selectPaymentQr(event: Event) {
  const input = event.target
  if (!(input instanceof HTMLInputElement)) return

  paymentSettingsError.value = ''
  paymentSettingsMessage.value = ''
  const file = input.files?.[0] ?? null
  if (paymentQrPreviewUrl.value) {
    URL.revokeObjectURL(paymentQrPreviewUrl.value)
    paymentQrPreviewUrl.value = ''
  }
  paymentQrFile.value = null

  if (!file) return
  if (!['image/png', 'image/jpeg', 'image/webp'].includes(file.type)) {
    paymentSettingsError.value = 'Upload a PNG, JPEG, or WebP QR image.'
    input.value = ''
    return
  }
  if (file.size === 0 || file.size > 5 * 1024 * 1024) {
    paymentSettingsError.value = 'QR image must be smaller than 5 MB.'
    input.value = ''
    return
  }

  paymentQrFile.value = file
  paymentQrPreviewUrl.value = URL.createObjectURL(file)
}

async function savePaymentSettings() {
  if (isSavingPaymentSettings.value) return

  paymentSettingsError.value = ''
  paymentSettingsMessage.value = ''
  isSavingPaymentSettings.value = true
  try {
    const settings = await updateAdminPaymentSettings(
      abaPaymentEnabled.value,
      paymentAbaUrl.value.trim(),
      paymentQrFile.value,
    )
    abaPaymentEnabled.value = settings.abaPaymentEnabled
    paymentAbaUrl.value = settings.abaUrl ?? ''
    paymentQrUrl.value = settings.qrImageUrl
    hasUploadedPaymentQr.value = settings.hasUploadedQr
    paymentQrFile.value = null
    if (paymentQrPreviewUrl.value) {
      URL.revokeObjectURL(paymentQrPreviewUrl.value)
      paymentQrPreviewUrl.value = ''
    }
    if (paymentQrInput.value) paymentQrInput.value.value = ''
    paymentSettingsMessage.value = 'Payment settings saved.'
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      await router.replace({ name: 'admin-login' })
      return
    }
    paymentSettingsError.value =
      error instanceof Error ? error.message : 'Unable to save payment settings.'
  } finally {
    isSavingPaymentSettings.value = false
  }
}

async function loadGuests() {
  isLoadingGuests.value = true
  guestListError.value = ''

  try {
    guests.value = await getGuests()
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      await router.replace({ name: 'admin-login' })
      return
    }
    guestListError.value =
      error instanceof Error ? error.message : 'Unable to load guest invitations.'
  } finally {
    isLoadingGuests.value = false
  }
}

async function signOut() {
  if (isSigningOut.value) return

  isSigningOut.value = true
  errorMessage.value = ''
  try {
    await logout()
    await router.replace({ name: 'admin-login' })
  } catch (error) {
    errorMessage.value =
      error instanceof Error ? error.message : 'Unable to sign out. Please try again.'
  } finally {
    isSigningOut.value = false
  }
}

async function addGuest() {
  if (isCreatingGuest.value) return

  guestError.value = ''
  createdGuest.value = null
  copiedInvitation.value = false
  isCreatingGuest.value = true

  try {
    createdGuest.value = await createGuest({
      name: guestName.value.trim(),
      ...(guestPhone.value.trim() ? { phone: guestPhone.value.trim() } : {}),
    })
    guestName.value = ''
    guestPhone.value = ''
    await loadGuests()
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      await router.replace({ name: 'admin-login' })
      return
    }
    guestError.value =
      error instanceof Error ? error.message : 'Unable to create the guest invitation.'
  } finally {
    isCreatingGuest.value = false
  }
}

async function copyInvitationLink() {
  if (!createdGuest.value) return

  try {
    await navigator.clipboard.writeText(createdGuest.value.invitationUrl)
    copiedInvitation.value = true
  } catch {
    guestError.value = 'Could not copy the link. Select and copy it manually.'
  }
}

async function copyGuestLink(guest: GuestListItem) {
  if (!guest.invitationUrl) return

  guestCopyError.value = ''
  try {
    await navigator.clipboard.writeText(guest.invitationUrl)
    copiedGuestId.value = guest.id
  } catch {
    guestCopyError.value = 'Could not copy the link. Open the invitation to copy it manually.'
  }
}

async function downloadGuestQr(guest: GuestListItem) {
  if (!guest.invitationUrl || qrDownloadingGuestId.value !== null) return

  qrDownloadError.value = ''
  qrDownloadingGuestId.value = guest.id
  try {
    const qrDataUrl = await QRCode.toDataURL(guest.invitationUrl, {
      errorCorrectionLevel: 'M',
      margin: 2,
      width: 512,
      type: 'image/png',
      color: {
        dark: '#62694a',
        light: '#fffefa',
      },
    })
    const downloadLink = document.createElement('a')
    const safeGuestName = guest.name
      .normalize('NFKD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-zA-Z0-9]+/g, '-')
      .replace(/^-|-$/g, '')

    downloadLink.href = qrDataUrl
    downloadLink.download = `invitation-${safeGuestName || guest.id}-qr.png`
    document.body.append(downloadLink)
    downloadLink.click()
    downloadLink.remove()
  } catch (error) {
    qrDownloadError.value =
      error instanceof Error
        ? `Could not generate the QR code: ${error.message}`
        : 'Could not generate the QR code.'
  } finally {
    qrDownloadingGuestId.value = null
  }
}

async function removeGuest(guest: GuestListItem) {
  if (deletingGuestId.value !== null) return
  if (!window.confirm(`Delete the invitation for ${guest.name}? This cannot be undone.`)) {
    return
  }

  deleteGuestError.value = ''
  deletingGuestId.value = guest.id
  try {
    await deleteGuest(guest.id)
    guests.value = guests.value.filter((item) => item.id !== guest.id)
    if (createdGuest.value?.id === guest.id) {
      createdGuest.value = null
    }
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      await router.replace({ name: 'admin-login' })
      return
    }
    deleteGuestError.value =
      error instanceof Error ? error.message : 'Unable to delete this guest.'
  } finally {
    deletingGuestId.value = null
  }
}

function selectInvitationLink(event: FocusEvent) {
  if (event.target instanceof HTMLInputElement) {
    event.target.select()
  }
}

onBeforeUnmount(() => {
  if (paymentQrPreviewUrl.value) URL.revokeObjectURL(paymentQrPreviewUrl.value)
})
</script>

<template>
  <main class="admin-page admin-dashboard-page">
    <div class="admin-dashboard">
      <header class="admin-dashboard-header">
        <a class="admin-brand" href="/" aria-label="Wedding invitation home">
          <span class="admin-brand-mark" aria-hidden="true">M<span>&</span>S</span>
          <span>Wedding administration</span>
        </a>
        <button
          class="admin-signout-button"
          type="button"
          :disabled="isSigningOut || isLoading || !admin"
          @click="signOut"
        >
          {{ isSigningOut ? 'Signing out…' : 'Sign out' }}
        </button>
      </header>

      <section class="admin-dashboard-content" aria-labelledby="admin-dashboard-heading">
        <p class="admin-eyebrow">Administrator portal</p>
        <h1 id="admin-dashboard-heading">Dashboard</h1>

        <p v-if="isLoading" class="admin-status" role="status">
          Checking your secure session…
        </p>
        <p v-else-if="errorMessage" class="admin-alert" role="alert">
          {{ errorMessage }}
        </p>
        <template v-else-if="admin">
          <p class="admin-welcome">Signed in as <strong>{{ admin.username }}</strong>.</p>

       

          <section class="admin-guest-panel" aria-labelledby="add-guest-heading">
            <div class="admin-panel-heading">
              <p class="admin-eyebrow">Personal invitations</p>
              <h2 id="add-guest-heading">Invite a guest</h2>
              <p>Create a personal invitation link to share with your guest.</p>
            </div>

            <form class="admin-form admin-guest-form" @submit.prevent="addGuest">
              <label for="guest-name">Guest name</label>
              <input
                id="guest-name"
                v-model="guestName"
                name="name"
                type="text"
                maxlength="200"
                autocomplete="name"
                placeholder="e.g. Dara and family"
                required
                :disabled="isCreatingGuest"
              />

              <label for="guest-phone">Phone number <span>(optional)</span></label>
              <input
                id="guest-phone"
                v-model="guestPhone"
                name="phone"
                type="tel"
                maxlength="50"
                autocomplete="tel"
                placeholder="e.g. +855 12 345 678"
                :disabled="isCreatingGuest"
              />

              <p v-if="guestError" class="admin-alert" role="alert">{{ guestError }}</p>

              <button
                class="admin-primary-button"
                type="submit"
                :disabled="isCreatingGuest || !guestName.trim()"
              >
                {{ isCreatingGuest ? 'Creating invitation…' : 'Create invitation link' }}
              </button>
            </form>

            <div v-if="createdGuest" class="admin-created-guest" role="status">
              <div>
                <p class="admin-card-label">Invitation created</p>
                <h3>{{ createdGuest.name }}</h3>
              </div>
              <label class="admin-link-label" for="created-invitation-link">
                Personal invitation link
              </label>
              <div class="admin-link-row">
                <input
                  id="created-invitation-link"
                  :value="createdGuest.invitationUrl"
                  readonly
                  @focus="selectInvitationLink"
                />
                <button class="admin-copy-button" type="button" @click="copyInvitationLink">
                  {{ copiedInvitation ? 'Copied' : 'Copy link' }}
                </button>
              </div>
              <a
                class="admin-text-link"
                :href="createdGuest.invitationUrl"
                target="_blank"
                rel="noopener noreferrer"
              >
                Preview invitation <span aria-hidden="true">↗</span>
              </a>
            </div>
          </section>

          <section class="admin-payment-panel" aria-labelledby="payment-settings-heading">
            <div class="admin-panel-heading">
              <p class="admin-eyebrow">Guest payments</p>
              <h2 id="payment-settings-heading">ABA payment settings</h2>
              <p>Enable or disable ABA payment, upload the QR code, and set its payment link.</p>
            </div>
            <p v-if="isLoadingPaymentSettings" class="admin-status" role="status">
              Loading payment settings…
            </p>
            <form v-else class="admin-payment-form" @submit.prevent="savePaymentSettings">
              <label class="admin-payment-toggle" for="aba-payment-enabled">
                <input
                  id="aba-payment-enabled"
                  v-model="abaPaymentEnabled"
                  type="checkbox"
                  :disabled="isSavingPaymentSettings"
                />
                <span>Enable ABA payment on guest invitations</span>
              </label>
              <label for="aba-payment-url">ABA payment URL</label>
              <input
                id="aba-payment-url"
                v-model="paymentAbaUrl"
                type="url"
                inputmode="url"
                placeholder="https://..."
                autocomplete="url"
                :disabled="isSavingPaymentSettings || !abaPaymentEnabled"
              />
              <label for="aba-payment-qr">Payment QR image (PNG, JPEG, or WebP; max 5 MB)</label>
              <input
                id="aba-payment-qr"
                ref="paymentQrInput"
                type="file"
                accept="image/png,image/jpeg,image/webp"
                :disabled="isSavingPaymentSettings || !abaPaymentEnabled"
                @change="selectPaymentQr"
              />
              <div class="admin-payment-preview">
                <img
                  :src="paymentQrPreviewUrl || paymentQrUrl"
                  alt="Current ABA payment QR code"
                />
                <span>
                  {{
                    paymentQrPreviewUrl
                      ? 'New QR selected'
                      : hasUploadedPaymentQr
                        ? 'Uploaded QR'
                        : 'Default QR'
                  }}
                </span>
              </div>
              <p v-if="paymentSettingsError" class="admin-alert" role="alert">
                {{ paymentSettingsError }}
              </p>
              <p v-if="paymentSettingsMessage" class="admin-success" role="status">
                {{ paymentSettingsMessage }}
              </p>
              <button
                class="admin-primary-button"
                type="submit"
                :disabled="isSavingPaymentSettings"
              >
                {{ isSavingPaymentSettings ? 'Saving…' : 'Save payment settings' }}
              </button>
            </form>
          </section>

          <section class="admin-guest-list" aria-labelledby="guest-list-heading">
            <div class="admin-guest-list-heading">
              <div>
                <p class="admin-eyebrow">Guest management</p>
                <h2 id="guest-list-heading">Guest list</h2>
                <p>{{ guests.length }} {{ guests.length === 1 ? 'guest' : 'guests' }}</p>
              </div>
              <button
                class="admin-refresh-button"
                type="button"
                :disabled="isLoadingGuests"
                @click="loadGuests"
              >
                {{ isLoadingGuests ? 'Refreshing…' : 'Refresh list' }}
              </button>
            </div>

            <p v-if="guestListError" class="admin-alert" role="alert">
              {{ guestListError }}
            </p>
            <p
              v-if="guestCopyError"
              class="admin-alert"
              role="alert"
            >
              {{ guestCopyError }}
            </p>
            <p v-if="deleteGuestError" class="admin-alert" role="alert">
              {{ deleteGuestError }}
            </p>
            <p v-if="qrDownloadError" class="admin-alert" role="alert">
              {{ qrDownloadError }}
            </p>
            <p v-if="isLoadingGuests && guests.length === 0" class="admin-status" role="status">
              Loading guest list…
            </p>
            <p v-else-if="!guestListError && guests.length === 0" class="admin-empty-guests">
              No guests yet. Create an invitation above to get started.
            </p>
            <div v-if="guests.length > 0" class="admin-guest-table-wrap">
              <table class="admin-guest-table">
                <thead>
                  <tr>
                    <th scope="col">Guest</th>
                    <th scope="col">Phone</th>
                    <th scope="col">Wish</th>
                    <th scope="col">RSVP</th>
                    <th scope="col">Invitation</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="guest in guests" :key="guest.id">
                    <td>
                      <span class="admin-guest-name">{{ guest.name }}</span>
                      <span class="admin-guest-id">#{{ guest.id }}</span>
                    </td>
                    <td>{{ guest.phone || '—' }}</td>
                    <td>
                      <p v-if="guest.wish" class="admin-guest-wish">{{ guest.wish }}</p>
                      <span v-else class="admin-no-wish">No wish yet</span>
                    </td>
                    <td>
                      <span v-if="guest.rsvpGuestCount === null" class="admin-rsvp-badge is-pending">
                        Pending
                      </span>
                      <span
                        v-else-if="guest.rsvpGuestCount === 0"
                        class="admin-rsvp-badge is-declined"
                      >
                        Declined
                      </span>
                      <span v-else class="admin-rsvp-badge is-confirmed">
                        {{ guest.rsvpGuestCount }} attending
                      </span>
                    </td>
                    <td>
                      <div class="admin-guest-actions">
                        <template v-if="guest.invitationUrl">
                          <a
                            class="admin-text-link"
                            :href="guest.invitationUrl"
                            target="_blank"
                            rel="noopener noreferrer"
                            :aria-label="`Open ${guest.name}'s invitation`"
                          >
                            Open
                          </a>
                          <button
                            class="admin-table-copy"
                            type="button"
                            @click="copyGuestLink(guest)"
                          >
                            {{ copiedGuestId === guest.id ? 'Copied' : 'Copy' }}
                          </button>
                          <button
                            class="admin-table-qr"
                            type="button"
                            :disabled="qrDownloadingGuestId !== null"
                            :aria-label="`Download QR code for ${guest.name}'s invitation`"
                            @click="downloadGuestQr(guest)"
                          >
                            {{ qrDownloadingGuestId === guest.id ? 'Preparing…' : 'QR / Download' }}
                          </button>
                        </template>
                        <span v-else class="admin-no-link">No link</span>
                        <button
                          class="admin-table-delete"
                          type="button"
                          :disabled="deletingGuestId !== null"
                          :aria-label="`Delete invitation for ${guest.name}`"
                          @click="removeGuest(guest)"
                        >
                          {{ deletingGuestId === guest.id ? 'Deleting…' : 'Delete' }}
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>
        </template>
      </section>
    </div>
  </main>
</template>
