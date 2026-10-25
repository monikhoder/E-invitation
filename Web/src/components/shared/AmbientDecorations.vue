<script setup lang="ts">
defineProps<{ variant?: 'cover' | 'body' }>()

function petalStyle(index: number) {
  return {
    '--left': `${(index * 37) % 100}%`,
    '--top': `${(index * 53) % 98}%`,
    '--size': `${7 + (index % 4)}px`,
    '--duration': `${5 + (index % 5)}s`,
    '--delay': `${(index % 7) * -0.8}s`,
  }
}
</script>

<template>
  <div class="petals" :class="{ 'body-petals': variant === 'body' }" aria-hidden="true">
    <i v-for="petal in 18" :key="petal" :style="petalStyle(petal)"></i>
  </div>
  <div
    class="cover-butterflies"
    :class="{ 'body-butterflies': variant === 'body' }"
    aria-hidden="true"
  >
    <svg
      v-for="butterfly in 3"
      :key="butterfly"
      class="cover-butterfly"
      :class="`butterfly-${['one', 'two', 'three'][butterfly - 1]}`"
      viewBox="0 0 48 42"
    >
      <path
        class="wing"
        d="M24 20C18 11 8 2 3 7c-5 6 1 17 18 20Zm0 3C15 18 8 25 12 33c3 6 9 1 12-6Z"
      />
      <path
        class="wing"
        d="M24 20C30 11 40 2 45 7c5 6-1 17-18 20Zm0 3c9-5 16 2 12 10-3 6-9 1-12-6Z"
      />
      <path class="body" d="M24 18v17m0-16-4-5m4 5 4-5" />
    </svg>
  </div>
</template>
