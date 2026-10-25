<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'

interface Photo {
  src: string
  alt: string
  className?: string
}

const gallery: Photo[] = [
  { src: '/images/monisreymao 1.jpg', alt: 'The couple sharing a quiet moment in a garden', className: 'gallery-wide' },
  { src: '/images/monisreymao 2.jpg', alt: 'A playful portrait of the couple' },
  { src: '/images/monisreymao 3.jpg', alt: 'The couple together by the sea' },
  { src: '/images/monisreymao 4.jpg', alt: 'The couple in traditional wedding attire' },
  { src: '/images/monisreymao 5.jpg', alt: 'The couple celebrating in traditional attire' },
]

const selectedPhoto = ref<Photo | null>(null)

function changePhoto(direction: number) {
  const index = gallery.findIndex((photo) => photo.src === selectedPhoto.value?.src)
  if (index < 0) return
  selectedPhoto.value = gallery[(index + direction + gallery.length) % gallery.length] ?? null
}

function handleKeydown(event: KeyboardEvent) {
  if (!selectedPhoto.value) return
  if (event.key === 'Escape') selectedPhoto.value = null
  if (event.key === 'ArrowLeft') changePhoto(-1)
  if (event.key === 'ArrowRight') changePhoto(1)
}

onMounted(() => window.addEventListener('keydown', handleKeydown))
onBeforeUnmount(() => window.removeEventListener('keydown', handleKeydown))
</script>

<template>
  <section class="gallery-section paper framed-section">
    <div class="section-inner">
      <p class="eyebrow khmer">អនុស្សាវរីយ៍របស់យើង</p>
      <h2 class="section-title">Our moments</h2>
      <p class="section-subtitle khmer">រាល់ថ្ងៃជាមួយគ្នា គឺជារឿងដ៏ស្រស់ស្អាត</p>
      <div class="photo-grid">
        <figure
          v-for="photo in gallery"
          :key="photo.src"
          :class="photo.className"
          role="button"
          tabindex="0"
          :aria-label="`View ${photo.alt}`"
          @click="selectedPhoto = photo"
          @keydown.enter.prevent="selectedPhoto = photo"
          @keydown.space.prevent="selectedPhoto = photo"
        >
          <img :src="photo.src" :alt="photo.alt" loading="lazy" />
        </figure>
      </div>
    </div>
    <div
      v-if="selectedPhoto"
      class="photo-viewer"
      role="dialog"
      aria-modal="true"
      :aria-label="selectedPhoto.alt"
      @click.self="selectedPhoto = null"
    >
      <button class="photo-viewer-close" type="button" aria-label="Close photo" @click="selectedPhoto = null">×</button>
      <button class="photo-viewer-arrow photo-viewer-prev" type="button" aria-label="Previous photo" @click.stop="changePhoto(-1)">‹</button>
      <button class="photo-viewer-arrow photo-viewer-next" type="button" aria-label="Next photo" @click.stop="changePhoto(1)">›</button>
      <img :src="selectedPhoto.src" :alt="selectedPhoto.alt" @click.stop />
    </div>
  </section>
</template>
