<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import ClosingSection from '../components/sections/ClosingSection.vue'
import CoverSection from '../components/sections/CoverSection.vue'
import GuestbookSection from '../components/sections/GuestbookSection.vue'
import InvitationDetailsSection from '../components/sections/InvitationDetailsSection.vue'
import IntroVideo from '../components/sections/IntroVideo.vue'
import MusicToggle from '../components/sections/MusicToggle.vue'
import VenueSection from '../components/sections/VenueSection.vue'
import WeddingProgramSection from '../components/sections/WeddingProgramSection.vue'
import AmbientDecorations from '../components/shared/AmbientDecorations.vue'
import PhotoGallerySection from '../components/shared/PhotoGallerySection.vue'
import RsvpSection from '../components/sections/RsvpSection.vue'
import PaymentSection from '../components/sections/PaymentSection.vue'
import { getGuestInvitation } from '../services/guests'

const route = useRoute()
const guestInvitationToken = computed(() =>
  typeof route.params.token === 'string' ? route.params.token : undefined,
)
const hasGuestInvitation = computed(() => guestInvitationToken.value !== undefined)
const guestName = ref('')
const guestWish = ref<string | null>(null)
const guestRsvpCount = ref<number | null>(null)
const isGuestLoading = ref(false)
const guestLoadError = ref('')
const isOpened = ref(false)
const isPlayingIntro = ref(false)
const musicEnabled = ref(true)
const openedScroll = ref<HTMLElement | null>(null)
let revealObserver: IntersectionObserver | undefined

watch(
  () => route.params.token,
  async (token, _previousToken, onCleanup) => {
    let isCurrentRequest = true
    onCleanup(() => {
      isCurrentRequest = false
    })

    guestLoadError.value = ''
    if (typeof token !== 'string') {
      guestName.value = ''
      guestWish.value = null
      guestRsvpCount.value = null
      isGuestLoading.value = false
      return
    }

    isGuestLoading.value = true
    guestWish.value = null
    guestRsvpCount.value = null
    try {
      const guest = await getGuestInvitation(token)
      if (isCurrentRequest) {
        guestName.value = guest.name
        guestWish.value = guest.wish
        guestRsvpCount.value = guest.rsvpGuestCount
      }
    } catch (error) {
      if (isCurrentRequest) {
        guestLoadError.value =
          error instanceof Error ? error.message : 'Unable to load this invitation.'
      }
    } finally {
      if (isCurrentRequest) isGuestLoading.value = false
    }
  },
  { immediate: true },
)

function openInvitation() {
  isPlayingIntro.value = true
}

function showInvitation() {
  isPlayingIntro.value = false
  isOpened.value = true
}

watch(
  isOpened,
  async (opened) => {
    revealObserver?.disconnect()
    if (!opened) return
    await nextTick()
    const root = openedScroll.value
    if (!root) return
    const items = root.querySelectorAll<HTMLElement>(
      '.section-inner > :not(.photo-grid), .photo-grid figure, .count-card, .timeline-item, .map-card, .message-card',
    )
    items.forEach((item, index) => {
      item.classList.add('scroll-reveal')
      item.style.setProperty('--reveal-delay', `${(index % 5) * 65}ms`)
    })
    if (!('IntersectionObserver' in window)) {
      items.forEach((item) => item.classList.add('is-visible'))
      return
    }
    revealObserver = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (!entry.isIntersecting) return
          entry.target.classList.add('is-visible')
          revealObserver?.unobserve(entry.target)
        })
      },
      { root, threshold: 0.12 },
    )
    items.forEach((item) => revealObserver?.observe(item))
  },
  { flush: 'post' },
)

onBeforeUnmount(() => revealObserver?.disconnect())
</script>

<template>
  <main class="invitation">
    <p v-if="isGuestLoading" role="status">Loading your invitation…</p>
    <p v-else-if="guestLoadError" role="alert">{{ guestLoadError }}</p>
    <template v-else>
      <AmbientDecorations />
      <MusicToggle v-model:enabled="musicEnabled" />

      <Transition name="cover-open">
        <CoverSection
          v-if="!isOpened && !isPlayingIntro"
          key="cover"
          :guest-name="guestName"
          @open="openInvitation"
        />
        <IntroVideo
          v-else-if="isPlayingIntro"
          key="intro"
          src="/%20video/save_date.MP4"
          @finished="showInvitation"
        />
        <section v-else key="invitation" class="opened-page">
          <AmbientDecorations variant="body" />
          <div ref="openedScroll" class="opened-scroll">
            <InvitationDetailsSection />
            <WeddingProgramSection />
            <VenueSection />
            <PhotoGallerySection />
            <RsvpSection
              v-if="hasGuestInvitation"
              :guest-token="guestInvitationToken"
              :guest-name="guestName"
              :initial-guest-count="guestRsvpCount"
            />
            <PaymentSection />
            <GuestbookSection
              v-if="hasGuestInvitation"
              :guest-token="guestInvitationToken"
              :guest-name="guestName"
              :initial-wish="guestWish"
            />
            <ClosingSection />
          </div>
          <button
            class="cover-again"
            type="button"
            aria-label="ត្រឡប់ទៅកាន់សំបកលិខិត"
            @click="isOpened = false"
          >
            ×
          </button>
        </section>
      </Transition>
    </template>
  </main>
</template>
