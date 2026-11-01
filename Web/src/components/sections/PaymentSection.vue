<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getPublicPaymentSettings, type PaymentSettings } from '../../services/payment'

const settings = ref<PaymentSettings | null>(null)
const errorMessage = ref('')
const isLoading = ref(true)

onMounted(async () => {
  try {
    settings.value = await getPublicPaymentSettings()
  } catch (error) {
    errorMessage.value =
      error instanceof Error ? error.message : 'Unable to load payment information.'
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <section
    v-if="isLoading || errorMessage || settings?.abaPaymentEnabled"
    class="payment-section paper framed-section"
  >
    <div class="section-inner">
      <p class="eyebrow khmer">ចូលរួមចងដៃ</p>
      <h2 class="khmer heading-khmer">សូមអញ្ជើញចងដៃតាមទឹកចិត្ត</h2>
      <p class="khmer payment-description">
        លោកអ្នកអាចស្កេន QR ខាងក្រោម ឬចុចប៊ូតុង ABA ដើម្បីបង់ប្រាក់។
      </p>
      <p v-if="isLoading" class="khmer" role="status">កំពុងផ្ទុកព័ត៌មានបង់ប្រាក់…</p>
      <p v-else-if="errorMessage" class="khmer payment-error" role="alert">
        {{ errorMessage }}
      </p>
      <div v-else-if="settings" class="payment-options">
        <img class="payment-qr" :src="settings.qrImageUrl" alt="ABA payment QR code" />
        <a
          v-if="settings.abaUrl"
          class="payment-link khmer"
          :href="settings.abaUrl"
          target="_blank"
          rel="noopener noreferrer"
        >
          បើក ABA ដើម្បីបង់ប្រាក់ <span aria-hidden="true">↗</span>
        </a>
        <p v-else class="khmer payment-link-unavailable">
          សូមស្កេន QR ដើម្បីបង់ប្រាក់។
        </p>
      </div>
    </div>
  </section>
</template>
