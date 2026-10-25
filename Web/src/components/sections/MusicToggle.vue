<script setup lang="ts">
defineProps<{ enabled: boolean }>()
defineEmits<{ 'update:enabled': [value: boolean] }>()
</script>

<template>
  <button
    class="music-toggle"
    type="button"
    :aria-label="enabled ? 'បិទសំឡេង' : 'បើកសំឡេង'"
    :aria-pressed="enabled"
    @click="$emit('update:enabled', !enabled)"
  >
    <svg v-if="enabled" viewBox="0 0 24 24" width="19" height="19" aria-hidden="true">
      <path
        fill="none"
        stroke="currentColor"
        stroke-width="1.8"
        stroke-linecap="round"
        stroke-linejoin="round"
        d="M4 10v4h4l5 4V6l-5 4H4Zm12-2a6 6 0 0 1 0 8m2-11a10 10 0 0 1 0 14"
      />
    </svg>
    <svg v-else viewBox="0 0 24 24" width="19" height="19" aria-hidden="true">
      <path
        fill="none"
        stroke="currentColor"
        stroke-width="1.8"
        stroke-linecap="round"
        stroke-linejoin="round"
        d="M4 10v4h4l5 4V6l-5 4H4Zm12 1 5 5m0-5-5 5"
      />
    </svg>
  </button>
</template>
