<script setup lang="ts">
import { onMounted, ref } from 'vue'

defineProps<{ src: string }>()
const emit = defineEmits<{ finished: [] }>()
const video = ref<HTMLVideoElement | null>(null)

onMounted(async () => {
  try {
    await video.value?.play()
  } catch (error) {
    console.warn('Video autoplay was blocked; use the video controls to start playback.', error)
  }
})

function handleVideoError(event: Event) {
  console.error('Could not load the save-date video:', event)
}
</script>

<template>
  <section class="intro-video-page" aria-label="Save the date video">
    <video
      ref="video"
      class="intro-video"
      :src="src"
      playsinline
      @ended="emit('finished')"
      @error="handleVideoError"
    ></video>
    <button class="skip-intro" type="button" @click="emit('finished')">រំលងវីដេអូ</button>
  </section>
</template>
