<script setup lang="ts">
import { reactive, ref, watch } from "vue";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import DatePicker from "primevue/datepicker";
import Textarea from "primevue/textarea";
import Message from "primevue/message";
import FormField from "../form/FormField.vue";
import { dialogShellPt, dialogSecondaryButtonClass, dialogPrimaryButtonClass } from "./dialogStyles";
import type { AuthorDto } from "../../../types";

export interface EditAuthorPayload {
  name: string;
  nationality: string;
  birthDate: string;
  bio: string;
}

const props = defineProps<{
  visible: boolean;
  author: AuthorDto | null;
  loading?: boolean;
  error?: string | null;
}>();

const emit = defineEmits<{
  "update:visible": [value: boolean];
  submit: [payload: EditAuthorPayload];
  cancel: [];
}>();

function formFromAuthor(author: AuthorDto | null) {
  return {
    name: author?.name ?? "",
    nationality: author?.nationality ?? "",
    birthDate: author ? new Date(author.birthDate) : (null as Date | null),
    bio: author?.bio ?? "",
  };
}

const form = reactive(formFromAuthor(props.author));
const validationError = ref<string | null>(null);

watch(
  () => props.visible,
  (visible) => {
    if (visible) {
      Object.assign(form, formFromAuthor(props.author));
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

const dialogPt = dialogShellPt("w-[480px]");
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
    :pt="dialogPt"
  >
    <template #header>
      <span class="text-[21px] font-bold text-color">Edit author</span>
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
        :class="dialogSecondaryButtonClass"
        @click="onUpdateVisible(false)"
      />
      <Button
        label="Save"
        icon="pi pi-check"
        :loading="loading"
        :class="dialogPrimaryButtonClass"
        @click="onSave"
      />
    </template>
  </Dialog>
</template>
