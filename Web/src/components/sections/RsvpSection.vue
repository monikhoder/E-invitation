<script setup lang="ts">
import { ref, watch } from 'vue'
import { saveGuestRsvp } from '../../services/guests'

const props = defineProps<{
  guestToken?: string
  guestName: string
  initialGuestCount: number | null
}>()

const willAttend = ref(props.initialGuestCount !== 0)
const guestCount = ref(Math.max(1, props.initialGuestCount ?? 1))
const isSaving = ref(false)
const savedMessage = ref('')
const errorMessage = ref('')

function adjustGuestCount(amount: number) {
  guestCount.value = Math.min(5, Math.max(1, guestCount.value + amount))
}

watch(
  () => props.initialGuestCount,
  (count) => {
    willAttend.value = count !== 0
    guestCount.value = Math.min(5, Math.max(1, count ?? 1))
  },
)

async function submitRsvp() {
  if (!props.guestToken || isSaving.value) return

  isSaving.value = true
  savedMessage.value = ''
  errorMessage.value = ''
  try {
    const response = await saveGuestRsvp(
      props.guestToken,
      willAttend.value ? guestCount.value : 0,
    )
    savedMessage.value =
      response.guestCount === 0
        ? 'សូមអរគុណដែលបានជូនដំណឹង។'
        : `បានបញ្ជាក់ការចូលរួមសម្រាប់ ${response.guestCount} នាក់។`
  } catch (error) {
    errorMessage.value =
      error instanceof Error ? error.message : 'មិនអាចរក្សាទុកការឆ្លើយតបបានទេ។'
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <section class="rsvp paper framed-section">
    <div class="section-inner">
      <p class="eyebrow khmer">ការបញ្ជាក់ចូលរួម</p>
      <h2 class="khmer heading-khmer">សូមបញ្ជាក់ការចូលរួម</h2>
      <form v-if="guestToken" class="rsvp-form" @submit.prevent="submitRsvp">
        <p class="khmer rsvp-guest-name">{{ guestName }}</p>
        <label class="rsvp-choice khmer">
          <input v-model="willAttend" type="radio" :value="true" :disabled="isSaving" />
          ខ្ញុំនឹងចូលរួម
        </label>
        <label class="khmer rsvp-count-label" for="rsvp-guest-count">
          ចំនួនអ្នកចូលរួម (រួមទាំងខ្ញុំ)
        </label>
        <div class="rsvp-counter" :class="{ 'is-disabled': isSaving || !willAttend }">
          <button
            type="button"
            aria-label="Decrease guest count"
            :disabled="isSaving || !willAttend || guestCount <= 1"
            @click="adjustGuestCount(-1)"
          >
            −
          </button>
          <output id="rsvp-guest-count" class="rsvp-counter-value" aria-live="polite">
            {{ guestCount }}
          </output>
          <button
            type="button"
            aria-label="Increase guest count"
            :disabled="isSaving || !willAttend || guestCount >= 5"
            @click="adjustGuestCount(1)"
          >
            +
          </button>
        </div>
        <label class="rsvp-choice khmer">
          <input v-model="willAttend" type="radio" :value="false" :disabled="isSaving" />
          សូមអភ័យទោស ខ្ញុំមិនអាចចូលរួមបានទេ
        </label>
        <p v-if="savedMessage" class="khmer" role="status">{{ savedMessage }}</p>
        <p v-if="errorMessage" class="khmer" role="alert">{{ errorMessage }}</p>
        <button class="send-button khmer" type="submit" :disabled="isSaving">
          {{ isSaving ? 'កំពុងរក្សាទុក…' : 'បញ្ជាក់ការចូលរួម' }}
        </button>
      </form>
      <p v-else class="khmer" role="status">
        សូមប្រើតំណអញ្ជើញផ្ទាល់ខ្លួន ដើម្បីបញ្ជាក់ការចូលរួម។
      </p>
    </div>
  </section>
</template>
