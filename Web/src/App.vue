<script setup lang="ts">
import { nextTick, onBeforeUnmount, ref, watch } from 'vue'
import ClosingSection from './components/sections/ClosingSection.vue'
import CoverSection from './components/sections/CoverSection.vue'
import GuestbookSection from './components/sections/GuestbookSection.vue'
import InvitationDetailsSection from './components/sections/InvitationDetailsSection.vue'
import IntroVideo from './components/sections/IntroVideo.vue'
import MusicToggle from './components/sections/MusicToggle.vue'
import VenueSection from './components/sections/VenueSection.vue'
import WeddingProgramSection from './components/sections/WeddingProgramSection.vue'
import AmbientDecorations from './components/shared/AmbientDecorations.vue'
import PhotoGallerySection from './components/shared/PhotoGallerySection.vue'

const guestName = ref('Cutie Theavy')
const isOpened = ref(false)
const isPlayingIntro = ref(false)
const musicEnabled = ref(true)
const openedScroll = ref<HTMLElement | null>(null)
let revealObserver: IntersectionObserver | undefined

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
          <GuestbookSection />
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
  </main>
</template>
