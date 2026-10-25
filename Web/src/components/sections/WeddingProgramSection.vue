<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'

interface ProgramItem {
  time: string
  title: string
  symbol: string
}

interface ProgramDay {
  id: string
  label: string
  schedule: ProgramItem[]
}

const weddingDate = new Date('2027-01-24T17:00:00')
const activeProgramDay = ref('jan24')
const now = ref(Date.now())
let countdownTimer: number | undefined

const programDays: ProgramDay[] = [
  {
    id: 'jan23',
    label: 'សៅរិ៍ ២៣ មករា',
    schedule: [
      { time: '៣:០០ រសៀល', title: 'ពិធីសែនក្រុងពាលី', symbol: '♧' },
      { time: '៤:០០ រសៀល', title: 'ពិធីសូត្រមន្ដចម្រើនព្រះបរិត្ត', symbol: '✦' },
      { time: '៥:០០ ល្ងាច', title: 'ពិធីជុំពេលា និងជាវខាន់ស្លា', symbol: '◇' },
      { time: '៦:៣០ ល្ងាច', title: 'អញ្ជើញភ្ញៀវកិត្តិយសពិសារអាហារពេលល្ងាច', symbol: '♥' },
    ],
  },
  {
    id: 'jan24',
    label: 'អាទិត្យ ២៤ មករា',
    schedule: [
      { time: '៦:៣០ ព្រឹក', title: 'ជួបជុំភ្ញៀវកិត្តិយសដើម្បីរៀបចំហែរជំនួន', symbol: '♧' },
      { time: '៧:០០ ព្រឹក', title: 'ពិធីហែរជំនួនកំណត់ចូលរោងជ័យ', symbol: '♥' },
      { time: '៧:៣០ ព្រឹក', title: 'ពិធីចៅមហានិយាយជើងការ សែនព្រេន និងពិសារអាហារពេលព្រឹក', symbol: '◇' },
      { time: '៨:០០ ព្រឹក', title: 'ពិធីបំពាក់ចិញ្ជៀន', symbol: '✦' },
      { time: '៩:០០ ព្រឹក', title: 'ពិធីកាត់សក់បង្កក់សិរី កូនប្រុស កូនស្រី', symbol: '♧' },
      { time: '១១:០០ ព្រឹក', title: 'ពិធីសំពះផ្ទឹមចងដៃ បង្វិលពពិល និងព្រះចោងតោងស្បៃ', symbol: '♥' },
      { time: '១២:០០ ថ្ងៃត្រង់', title: 'អញ្ជើញភ្ញៀវកិត្តិយសពិសារអាហារពេលថ្ងៃត្រង់', symbol: '◇' },
      { time: '៥:០០ ល្ងាច', title: 'អញ្ជើញភ្ញៀវកិត្តិយសពិសាភោជនាហារដោយមេត្រីភាព', symbol: '✦' },
    ],
  },
]

const khmerDigits = ['០', '១', '២', '៣', '៤', '៥', '៦', '៧', '៨', '៩']
const countdown = computed(() => {
  const remaining = Math.max(0, weddingDate.getTime() - now.value)
  const values = [
    Math.floor(remaining / 86_400_000),
    Math.floor((remaining % 86_400_000) / 3_600_000),
    Math.floor((remaining % 3_600_000) / 60_000),
  ]
  return values.map((value, index) => ({
    value: String(value).replace(/\d/g, (digit) => khmerDigits[Number(digit)] ?? digit),
    label: ['ថ្ងៃ', 'ម៉ោង', 'នាទី'][index],
  }))
})

const selectedSchedule = computed(
  () => programDays.find((day) => day.id === activeProgramDay.value)?.schedule ?? [],
)

onMounted(() => {
  countdownTimer = window.setInterval(() => {
    now.value = Date.now()
  }, 60_000)
})

onUnmounted(() => {
  if (countdownTimer !== undefined) window.clearInterval(countdownTimer)
})
</script>

<template>
  <section class="countdown-section paper framed-section">
    <div class="section-inner">
      <h2 class="khmer heading-khmer">រាប់ថយក្រោយទៅកាន់ថ្ងៃមង្គលការ</h2>
      <div class="countdown-grid">
        <div v-for="unit in countdown" :key="unit.label" class="count-card">
          <strong>{{ unit.value }}</strong><span>{{ unit.label }}</span>
        </div>
      </div>
      <div class="rule"><span>✦</span></div>
      <p class="eyebrow khmer timeline-heading">កម្មវិធីពិធីមង្គលការ</p>
      <div class="program-day-switch" role="group" aria-label="Select wedding program date">
        <button
          v-for="day in programDays"
          :key="day.id"
          type="button"
          :class="{ active: activeProgramDay === day.id }"
          :aria-pressed="activeProgramDay === day.id"
          @click="activeProgramDay = day.id"
        >
          {{ day.label }}
        </button>
      </div>
      <Transition name="program-switch" mode="out-in">
        <div :key="activeProgramDay" class="timeline">
          <div v-for="item in selectedSchedule" :key="item.time" class="timeline-item">
            <div class="timeline-symbol" aria-hidden="true">{{ item.symbol }}</div>
            <div class="timeline-copy"><time>{{ item.time }}</time><p class="khmer">{{ item.title }}</p></div>
          </div>
        </div>
      </Transition>
    </div>
  </section>
</template>
