<script setup lang="ts">
import { reactive, ref, watch } from "vue";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import Select from "primevue/select";
import DatePicker from "primevue/datepicker";
import InputNumber from "primevue/inputnumber";
import Textarea from "primevue/textarea";
import Message from "primevue/message";
import FormField from "../form/FormField.vue";
import { dialogShellPt, dialogSecondaryButtonClass, dialogPrimaryButtonClass } from "./dialogStyles";
import type { AuthorOption } from "../../../types";

export interface CreateBookPayload {
  title: string;
  isbn: string;
  authorId: number;
  genre: string;
  publishedDate: string;
  price: number;
  stock: number;
  numberOfPages: number;
  description: string;
}

const props = defineProps<{
  visible: boolean;
  authors: AuthorOption[];
  loading?: boolean;
  error?: string | null;
}>();

const emit = defineEmits<{
  "update:visible": [value: boolean];
  submit: [payload: CreateBookPayload];
  cancel: [];
}>();

function emptyForm() {
  return {
    title: "",
    isbn: "",
    authorId: null as number | null,
    genre: "",
    publishedDate: null as Date | null,
    price: null as number | null,
    stock: null as number | null,
    numberOfPages: null as number | null,
    description: "",
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
  if (!form.title.trim() || !form.authorId) {
    validationError.value = "Title and Author are required.";
    return;
  }
  validationError.value = null;
  emit("submit", {
    title: form.title.trim(),
    isbn: form.isbn.trim(),
    authorId: form.authorId,
    genre: form.genre.trim(),
    publishedDate: (form.publishedDate ?? new Date()).toISOString(),
    price: form.price ?? 0,
    stock: form.stock ?? 0,
    numberOfPages: form.numberOfPages ?? 0,
    description: form.description.trim(),
  });
}

const dialogPt = dialogShellPt("w-[765px]");
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
      <span class="text-[21px] font-bold text-color">Create book</span>
    </template>

    <FormField label="Title">
      <InputText v-model="form.title" class="w-full" />
    </FormField>

    <FormField label="ISBN">
      <InputText v-model="form.isbn" class="w-full" />
    </FormField>

    <div class="flex w-full items-start gap-[21px]">
      <FormField label="Author">
        <Select
          v-model="form.authorId"
          :options="props.authors"
          option-label="name"
          option-value="id"
          placeholder="Select an author"
          class="w-full"
        />
      </FormField>
      <FormField label="Genre">
        <InputText v-model="form.genre" class="w-full" />
      </FormField>
      <FormField label="Published date">
        <DatePicker v-model="form.publishedDate" date-format="dd/mm/yy" show-icon class="w-full" />
      </FormField>
    </div>

    <div class="flex w-full items-start gap-[21px]">
      <FormField label="Price">
        <InputNumber v-model="form.price" mode="currency" currency="BRL" locale="pt-BR" class="w-full" />
      </FormField>
      <FormField label="Stock">
        <InputNumber v-model="form.stock" :min="0" class="w-full" />
      </FormField>
      <FormField label="Number of pages">
        <InputNumber v-model="form.numberOfPages" :min="1" class="w-full" />
      </FormField>
    </div>

    <div class="flex h-[138px] w-full flex-col gap-1.75">
      <label class="text-sm font-semibold text-color">Description</label>
      <Textarea v-model="form.description" class="h-[110px] w-full resize-none" />
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
