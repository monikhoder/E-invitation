<script setup lang="ts">
import { ref } from 'vue'

interface GuestMessage {
  id: number
  name: string
  text: string
}

const guestName = ref('Cutie Theavy')
const guestMessage = ref('')
const messages = ref<GuestMessage[]>([
  {
    id: 1,
    name: 'Sokroth',
    text: 'Congratulations on your wedding! Wishing you both a lifetime filled with love, laughter, and beautiful memories.',
  },
])

function addMessage() {
  const name = guestName.value.trim()
  const text = guestMessage.value.trim()
  if (!name || !text) return
  messages.value.unshift({ id: Date.now(), name, text })
  guestName.value = ''
  guestMessage.value = ''
}

function removeMessage(id: number) {
  messages.value = messages.value.filter((message) => message.id !== id)
}
</script>

<template>
  <section class="guestbook paper framed-section">
    <div class="section-inner">
      <p class="eyebrow khmer">សារជូនពរ</p>
      <h2 class="khmer heading-khmer">សូមចែករំលែកពាក្យជូនពរ</h2>
      <form class="guest-form" @submit.prevent="addMessage">
        <input
          v-model="guestName"
          class="khmer"
          required
          maxlength="50"
          placeholder="ឈ្មោះរបស់អ្នក..."
          aria-label="Your name"
        />
        <textarea
          v-model="guestMessage"
          class="khmer"
          required
          maxlength="300"
          rows="3"
          placeholder="សារជូនពររបស់អ្នក..."
          aria-label="Your message"
        ></textarea>
        <button class="send-button khmer" type="submit">ផ្ញើសារ <span aria-hidden="true">➤</span></button>
      </form>
      <div class="message-list">
        <article v-for="message in messages" :key="message.id" class="message-card">
          <div class="message-heading">
            <strong>{{ message.name }}</strong>
            <button type="button" :aria-label="`Delete message from ${message.name}`" @click="removeMessage(message.id)">×</button>
          </div>
          <p>{{ message.text }}</p>
          <time>With love · 24 មករា 2027</time>
        </article>
      </div>
    </div>
  </section>
</template>
