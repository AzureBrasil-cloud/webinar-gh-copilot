<script setup lang="ts">
import { reactive, ref, watch } from "vue";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import DatePicker from "primevue/datepicker";
import Textarea from "primevue/textarea";
import Message from "primevue/message";
import FormField from "../form/FormField.vue";

export interface CreateAuthorPayload {
  name: string;
  nationality: string;
  birthDate: string;
  bio: string;
}

const props = defineProps<{
  visible: boolean;
  loading?: boolean;
  error?: string | null;
}>();

const emit = defineEmits<{
  "update:visible": [value: boolean];
  submit: [payload: CreateAuthorPayload];
  cancel: [];
}>();

function emptyForm() {
  return {
    name: "",
    nationality: "",
    birthDate: null as Date | null,
    bio: "",
  };
}

const form = reactive(emptyForm());
const validationError = ref<string | null>(null);

watch(
  () => props.visible,
  (visible) => {
    if (visible) {
      Object.assign(form, emptyForm());
      validationError.value = null;
    }
  },
);

function onUpdateVisible(value: boolean) {
  emit("update:visible", value);
  if (!value) emit("cancel");
}

function onSave() {
  if (!form.name.trim() || !form.birthDate) {
    validationError.value = "Name and Birth date are required.";
    return;
  }
  validationError.value = null;
  emit("submit", {
    name: form.name.trim(),
    nationality: form.nationality.trim(),
    birthDate: form.birthDate.toISOString(),
    bio: form.bio.trim(),
  });
}
</script>

<template>
  <Dialog
    :visible="props.visible"
    @update:visible="onUpdateVisible"
    modal
    dismissable-mask
    :closable="!loading"
    :close-on-escape="!loading"
    :draggable="false"
    :pt="{
      root: { class: 'w-[480px] max-w-[92vw] rounded-[21px] border border-surface-300 bg-surface-0 p-0 overflow-hidden' },
      header: { class: 'h-[67px] items-center justify-between pt-5 pb-4 pl-6 pr-4' },
      content: { class: 'flex flex-col gap-[21px] px-6 py-2' },
      footer: { class: 'gap-3 justify-end pb-5 pt-4 px-6' },
    }"
  >
    <template #header>
      <span class="text-[21px] font-bold text-color">Create author</span>
    </template>

    <FormField label="Name">
      <InputText v-model="form.name" class="w-full" />
    </FormField>

    <FormField label="Nationality">
      <InputText v-model="form.nationality" class="w-full" />
    </FormField>

    <FormField label="Birth date">
      <DatePicker v-model="form.birthDate" date-format="dd/mm/yy" show-icon class="w-full" />
    </FormField>

    <div class="flex h-[138px] w-full flex-col gap-1.75">
      <label class="text-sm font-semibold text-color">Bio</label>
      <Textarea v-model="form.bio" class="h-[110px] w-full resize-none" />
    </div>

    <Message v-if="validationError || props.error" severity="error" :closable="false">
      {{ validationError ?? props.error }}
    </Message>

    <template #footer>
      <Button
        label="Cancel"
        severity="secondary"
        outlined
        :disabled="loading"
        class="gap-1.75! rounded-md! border-surface-300! px-[11.5px]! py-2! text-sm! font-medium!"
        @click="onUpdateVisible(false)"
      />
      <Button
        label="Save"
        icon="pi pi-check"
        :loading="loading"
        class="gap-1.75! rounded-md! border-surface-700! bg-surface-700! px-[11.5px]! py-2! text-sm! font-medium! hover:bg-surface-800!"
        @click="onSave"
      />
    </template>
  </Dialog>
</template>
