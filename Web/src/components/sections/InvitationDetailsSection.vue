<script setup lang="ts">
const couple = {
  groom: 'រិន មនិ',
  bride: 'វ៉ន ស្រីម៉ៅ',
}
</script>

<template>
  <section id="invitation-details" class="details paper framed-section">
    <div class="section-inner">
      <img class="details-logo" src="/images/sm_logo.png" alt="S and M wedding logo" />
      <h2 class="khmer moul heading-khmer">សិរីមង្គលអាពាហ៍ពិពាហ៍</h2>
      <div class="couple-parents">
        <div>
          <strong class="khmer moul">លោក ឈឹម រិន</strong>
          <br />
          <strong class="khmer moul">អ្នកស្រី សុន អូន</strong>
        </div>
        <div>
          <strong class="khmer moul">លោក អ៊ូ សាវឿន</strong>
          <br />
          <strong class="khmer moul">អ្នកស្រី ម៉ូន ជិន</strong>
        </div>
      </div>
      <h3 class="khmer moul heading-khmer">មានកិត្តិយសសូមគោរពអញ្ជើញ</h3>
      <p class="intro khmer">
        ឯកឧត្តម លោកឧកញ៉ា លោកជំទាវ លោក លោកស្រី អ្នកនាង កញ្ញា និងប្រិយមិត្ត អញ្ជើញចូលរួមជាអធិបតី
        និងជាភ្ញៀវកិត្តិយស ដើម្បីប្រសិទ្ធពរជ័យសិរីសួស្តី ជ័យមង្គលក្នុងពិធីរៀបអាពាហ៍ពិពាហ៍
        កូនប្រុស-កូនស្រីរបស់យើងខ្ញុំ។
      </p>
      <div class="couple-names">
        <div><span class="khmer moulpali">កូនប្រុសនាម</span><strong class="khmer moul">{{ couple.groom }}</strong></div>
        <span class="moulpali" aria-hidden="true">ជាគូនឹង</span>
        <div><span class="khmer moulpali">កូនស្រីនាម</span><strong class="khmer moul">{{ couple.bride }}</strong></div>
      </div>
      <p class="parents khmer">
        ពិសាភោជនាហារប្រព្រឹត្តទៅនៅថ្ងៃ អាទិត្យ ២រោច ខែបុស្ស ឆ្នាំមមី អដ្ឋស័ក ព.ស. ២៥៧០
        ត្រូវនឹងថ្ងៃទី២៤ ខែមករា ឆ្នាំ២០២៧ វេលាម៉ោង ៥:០០ ល្ងាច នៅគេហដ្ឋានខាងស្រី
        ភូមិស្វាយ ឃុំបារាយណ៍ ស្រុកបារាយណ៍ ខេត្តកំពង់ធំ ដោយមេត្រីភាព។
      </p>
      <div class="date-card">
        <span aria-hidden="true">▦</span>
        <span class="date-day">២៤</span>
        <div><span class="date-month">មករា</span><span class="date-year">អាទិត្យ · ២០២៧</span></div>
      </div>
    </div>
  </section>
</template>
