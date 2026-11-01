<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { getGuestWishes, saveGuestWish, type GuestWish } from '../../services/guests'

const props = withDefaults(
  defineProps<{
    guestToken?: string
    guestName: string
    initialWish?: string | null
  }>(),
  { initialWish: null },
)

const guestMessage = ref(props.initialWish ?? '')
const isSaving = ref(false)
const isSaved = ref(Boolean(props.initialWish))
const errorMessage = ref('')
const wishes = ref<GuestWish[]>([])
const wishLoadError = ref('')
const activeWishIndex = ref(0)
const activeWish = computed(() => wishes.value[activeWishIndex.value])
let slideTimer: ReturnType<typeof setInterval> | undefined

async function loadWishes() {
  wishLoadError.value = ''
  try {
    wishes.value = await getGuestWishes()
    if (activeWishIndex.value >= wishes.value.length) {
      activeWishIndex.value = 0
    }
  } catch (error) {
    wishLoadError.value = error instanceof Error ? error.message : 'Unable to load guest wishes.'
  }
}

function showPreviousWish() {
  if (wishes.value.length < 2) return
  activeWishIndex.value = (activeWishIndex.value - 1 + wishes.value.length) % wishes.value.length
}

function showNextWish() {
  if (wishes.value.length < 2) return
  activeWishIndex.value = (activeWishIndex.value + 1) % wishes.value.length
}

onMounted(() => {
  void loadWishes()
  slideTimer = setInterval(showNextWish, 6000)
})

onBeforeUnmount(() => {
  if (slideTimer) clearInterval(slideTimer)
})

watch(
  () => props.initialWish,
  (wish) => {
    guestMessage.value = wish ?? ''
    isSaved.value = Boolean(wish)
  },
)

async function addMessage() {
  if (!props.guestToken || isSaving.value) return

  const wish = guestMessage.value.trim()
  if (!wish) return

  isSaving.value = true
  isSaved.value = false
  errorMessage.value = ''
  try {
    guestMessage.value = await saveGuestWish(props.guestToken, wish)
    isSaved.value = true
    await loadWishes()
    const savedWishIndex = wishes.value.findIndex(
      (item) => item.name === props.guestName && item.wish === guestMessage.value,
    )
    if (savedWishIndex >= 0) activeWishIndex.value = savedWishIndex
  } catch (error) {
    errorMessage.value =
      error instanceof Error ? error.message : 'Unable to save your wish. Please try again.'
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <section class="guestbook paper framed-section">
    <div class="section-inner">
      <p class="eyebrow khmer">សារជូនពរ</p>
      <h2 class="khmer heading-khmer">សូមចែករំលែកពាក្យជូនពរ</h2>
      <form v-if="guestToken" class="guest-form" @submit.prevent="addMessage">
        <p class="khmer">{{ guestName }}</p>
        <textarea
          v-model="guestMessage"
          class="khmer"
          required
          maxlength="1000"
          rows="3"
          placeholder="សារជូនពររបស់អ្នក..."
          aria-label="Your message"
          :disabled="isSaving"
        ></textarea>
        <p v-if="errorMessage" class="khmer" role="alert">{{ errorMessage }}</p>
        <p v-if="isSaved" class="khmer" role="status">បានរក្សាទុកពាក្យជូនពររបស់អ្នក។</p>
        <button class="send-button khmer" type="submit" :disabled="isSaving">
          {{ isSaving ? 'កំពុងរក្សាទុក…' : 'ផ្ញើសារ' }} <span aria-hidden="true">➤</span>
        </button>
      </form>
      <p v-else class="khmer" role="status">សូមប្រើតំណអញ្ជើញផ្ទាល់ខ្លួន ដើម្បីផ្ញើពាក្យជូនពរ។</p>
      <div class="wish-slideshow" aria-label="Guest wishes">
        <h3 class="khmer">ពាក្យជូនពរពីភ្ញៀវ</h3>
        <p v-if="wishLoadError" class="khmer wish-slide-error" role="alert">
          {{ wishLoadError }}
        </p>
        <p v-else-if="wishes.length === 0" class="khmer wish-slide-empty">
          មិនទាន់មានពាក្យជូនពរទេ។
        </p>
        <template v-else-if="activeWish">
          <Transition name="wish-slide" mode="out-in">
            <article :key="activeWish.id" class="message-card wish-slide-card" aria-live="polite">
              <strong class="wish-slide-name">{{ activeWish.name }}</strong>
              <p class="wish-slide-text">{{ activeWish.wish }}</p>
            </article>
          </Transition>
          <div v-if="wishes.length > 1" class="wish-slide-controls">
            <button type="button" aria-label="Previous wish" @click="showPreviousWish">‹</button>
            <div class="wish-slide-dots" aria-label="Choose a wish">
              <button
                v-for="(wish, index) in wishes"
                :key="wish.id"
                type="button"
                :class="{ active: index === activeWishIndex }"
                :aria-label="`Show wish ${index + 1}`"
                :aria-current="index === activeWishIndex ? 'true' : undefined"
                @click="activeWishIndex = index"
              ></button>
            </div>
            <button type="button" aria-label="Next wish" @click="showNextWish">›</button>
          </div>
        </template>
      </div>
    </div>
  </section>
</template>
