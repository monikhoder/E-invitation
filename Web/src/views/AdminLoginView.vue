<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '../services/auth'

const router = useRouter()
const username = ref('')
const password = ref('')
const errorMessage = ref('')
const isSubmitting = ref(false)

async function submitLogin() {
  if (isSubmitting.value) return

  errorMessage.value = ''
  isSubmitting.value = true

  try {
    await login(username.value.trim(), password.value)
    await router.replace({ name: 'admin-dashboard' })
  } catch (error) {
    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to sign in. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <main class="admin-page">
    <section class="admin-login-card" aria-labelledby="admin-login-heading">
      <a class="admin-brand" href="/" aria-label="Wedding invitation home">
        <span class="admin-brand-mark" aria-hidden="true">M<span>&</span>S</span>
        <span>Wedding administration</span>
      </a>

      <div class="admin-login-intro">
        <p class="admin-eyebrow">Private access</p>
        <h1 id="admin-login-heading">Welcome back</h1>
        <p>Sign in with your administrator account to continue.</p>
      </div>

      <form class="admin-form" @submit.prevent="submitLogin">
        <label for="admin-username">Username</label>
        <input
          id="admin-username"
          v-model="username"
          name="username"
          type="text"
          autocomplete="username"
          required
          autofocus
          :disabled="isSubmitting"
        />

        <label for="admin-password">Password</label>
        <input
          id="admin-password"
          v-model="password"
          name="password"
          type="password"
          autocomplete="current-password"
          required
          :disabled="isSubmitting"
        />

        <p v-if="errorMessage" class="admin-alert" role="alert">
          {{ errorMessage }}
        </p>

        <button class="admin-primary-button" type="submit" :disabled="isSubmitting">
          {{ isSubmitting ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>

      <a class="admin-back-link" href="/">← Back to invitation</a>
    </section>
  </main>
</template>
