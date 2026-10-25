<script setup lang="ts">
defineProps<{ guestName: string }>()
defineEmits<{ open: [] }>()
</script>

<template>
  <section class="hero paper" aria-label="Wedding invitation cover">
    <div class="landscape" aria-hidden="true"></div>
    <span class="cover-door door-left" aria-hidden="true"></span>
    <span class="cover-door door-right" aria-hidden="true"></span>
    <div class="birds" aria-hidden="true">⌁ &nbsp; ⌁ &nbsp; ⌁</div>
    <div class="hero-content">
      <h1 class="cover-khmer khmer">សិរីមង្គលអាពាហ៍ពិពាហ៍</h1>
      <div class="crest" aria-label="S and M wedding logo">
        <img class="cover-logo" src="/images/sm_logo.png" alt="S and M wedding logo" />
      </div>
      <h2 class="cover-caption khmer">សូមគោរពអញ្ជើញ</h2>
      <p class="cover-names khmer">{{ guestName }}</p>
      <button type="button" class="open-invitation khmer" @click="$emit('open')">
        បើកលិខិតអញ្ជើញ
      </button>
      <p class="cover-note khmer">សូមអញ្ជើញចូលរួមអបអរសាទរថ្ងៃពិសេសរបស់យើង</p>
    </div>
  </section>
</template>
