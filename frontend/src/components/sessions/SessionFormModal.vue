<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-50 flex items-end sm:items-center justify-center">
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="$emit('close')" />
      <div class="relative bg-bg-elevated rounded-t-2xl sm:rounded-2xl w-full sm:max-w-lg max-h-[90vh] flex flex-col shadow-elevated">
        <div class="flex items-center justify-between px-5 pt-5 pb-4 border-b border-border-default flex-shrink-0">
          <h2 class="font-display text-2xl text-text-primary tracking-wide">
            {{ isEdit ? 'EDIT SESSION' : 'NEW SESSION' }}
          </h2>
          <button @click="$emit('close')" class="text-text-muted hover:text-text-primary transition">
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>

        <div class="flex-1 overflow-y-auto px-5 py-4">
          <div class="mb-4">
            <label class="block text-sm text-text-secondary mb-1">Date & Time</label>
            <input
              v-model="form.scheduledAt"
              type="datetime-local"
              class="w-full bg-bg-subtle border border-border-default rounded-lg px-3 py-2.5 text-text-primary focus:outline-none focus:border-primary transition text-sm"
              :class="{ 'border-red-500': submitted && !form.scheduledAt }"
            />
            <p v-if="submitted && !form.scheduledAt" class="mt-1 text-xs text-red-400">Date is required</p>
          </div>

          <div>
            <div class="flex items-center justify-between mb-2">
              <label class="text-sm text-text-secondary">Exercises</label>
              <span v-if="submitted && form.exercises.length === 0" class="text-xs text-red-400">
                Add at least 1 exercise
              </span>
            </div>

            <ExerciseRow
              v-for="(ex, i) in form.exercises"
              :key="i"
              :exercise="ex"
              :show-name-error="submitted"
              @update="(field, val) => updateExercise(i, field, val)"
              @remove="removeExercise(i)"
            />

            <button
              type="button"
              @click="addExercise"
              class="mt-3 w-full border border-dashed border-border-default rounded-lg py-3 text-sm text-text-secondary hover:text-text-primary hover:border-primary transition"
            >
              + Add Exercise
            </button>
          </div>
        </div>

        <div class="px-5 py-4 border-t border-border-default flex-shrink-0 space-y-2">
          <p v-if="submitError" class="text-sm text-red-400">{{ submitError }}</p>
          <div class="flex gap-3">
            <button
              v-if="isEdit"
              type="button"
              @click="showDeleteConfirm = true"
              class="px-4 py-2.5 border border-red-500/50 text-red-400 rounded-lg hover:bg-red-500/10 transition text-sm"
            >
              Delete
            </button>
            <button
              type="button"
              @click="$emit('close')"
              class="flex-1 border border-border-default text-text-secondary py-2.5 rounded-lg hover:bg-bg-subtle transition text-sm"
            >
              Cancel
            </button>
            <button
              @click="handleSave"
              :disabled="loading"
              class="flex-1 bg-primary text-text-inverse font-semibold py-2.5 rounded-lg hover:bg-primary-dark transition disabled:opacity-50 text-sm"
            >
              {{ loading ? 'Saving…' : 'Save' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <ConfirmDialog
      v-if="showDeleteConfirm"
      title="Delete session?"
      message="This session will be permanently removed."
      confirm-label="Delete"
      cancel-label="Keep"
      :destructive="true"
      @confirm="handleDelete"
      @cancel="showDeleteConfirm = false"
    />
  </Teleport>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { sessionsApi, type WorkoutSession } from '@/api/sessions'
import ExerciseRow from './ExerciseRow.vue'
import ConfirmDialog from '@/components/ui/ConfirmDialog.vue'

const props = defineProps<{
  clientId: string
  initialDate?: string
  session?: WorkoutSession
}>()

const emit = defineEmits<{ close: []; saved: []; deleted: [] }>()

const isEdit = !!props.session
const loading = ref(false)
const submitted = ref(false)
const submitError = ref('')
const showDeleteConfirm = ref(false)

type ExerciseForm = { name: string; sets: number | null; reps: number | null; targetWeight: number | null }

const form = reactive<{ scheduledAt: string; exercises: ExerciseForm[] }>({
  scheduledAt: '',
  exercises: []
})

onMounted(() => {
  if (props.session) {
    form.scheduledAt = props.session.scheduledAt.slice(0, 16)
    form.exercises = props.session.exercises.map((e) => ({
      name: e.name,
      sets: e.sets,
      reps: e.reps,
      targetWeight: e.targetWeight ?? null
    }))
  } else if (props.initialDate) {
    form.scheduledAt = props.initialDate.slice(0, 16)
    addExercise()
  } else {
    addExercise()
  }
})

function addExercise() {
  form.exercises.push({ name: '', sets: null, reps: null, targetWeight: null })
}

function removeExercise(index: number) {
  form.exercises.splice(index, 1)
}

function updateExercise(index: number, field: string, value: unknown) {
  ;(form.exercises[index] as Record<string, unknown>)[field] = value
}

function isValid(): boolean {
  submitted.value = true
  if (!form.scheduledAt) return false
  if (form.exercises.length === 0) return false
  return form.exercises.every(
    (e) => e.name.trim() && e.sets && e.sets > 0 && e.reps && e.reps > 0
  )
}

async function handleSave() {
  if (!isValid()) return
  loading.value = true
  submitError.value = ''
  try {
    const payload = {
      scheduledAt: new Date(form.scheduledAt).toISOString(),
      exercises: form.exercises.map((e) => ({
        name: e.name.trim(),
        sets: e.sets!,
        reps: e.reps!,
        targetWeight: e.targetWeight ?? null
      }))
    }
    if (isEdit && props.session) {
      await sessionsApi.update(props.clientId, props.session.id, payload)
    } else {
      await sessionsApi.create(props.clientId, payload)
    }
    emit('saved')
    emit('close')
  } catch (err: unknown) {
    const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
    submitError.value = msg ?? 'Failed to save session'
  } finally {
    loading.value = false
  }
}

async function handleDelete() {
  if (!props.session) return
  loading.value = true
  try {
    await sessionsApi.delete(props.clientId, props.session.id)
    emit('deleted')
    emit('close')
  } catch {
    submitError.value = 'Failed to delete session'
  } finally {
    loading.value = false
    showDeleteConfirm.value = false
  }
}
</script>
