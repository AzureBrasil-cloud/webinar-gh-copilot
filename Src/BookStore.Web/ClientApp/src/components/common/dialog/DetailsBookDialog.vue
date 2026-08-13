<script setup lang="ts">
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import Tag from "primevue/tag";
import DetailField from "../form/DetailField.vue";
import type { BookDto } from "../../../types";

const props = defineProps<{
  visible: boolean;
  book: BookDto | null;
}>();

const emit = defineEmits<{ "update:visible": [value: boolean] }>();

function formatCurrency(value: number) {
  return new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);
}

// API dates are ISO strings ("yyyy-MM-ddTHH:mm:ss"); slicing avoids a local-timezone shift.
function formatIsoDate(value: string) {
  return value.slice(0, 10);
}
</script>

<template>
  <Dialog
    :visible="props.visible"
    @update:visible="(value: boolean) => emit('update:visible', value)"
    modal
    dismissable-mask
    :draggable="false"
    :pt="{
      root: { class: 'w-[520px] max-w-[92vw] rounded-[21px] border border-surface-300 bg-surface-0 p-0 overflow-hidden' },
      header: { class: 'h-[67px] items-center justify-between pt-5 pb-4 pl-6 pr-4' },
      content: { class: 'flex flex-col gap-1.75 px-[21px] py-[17.5px]' },
      footer: { class: 'gap-3 justify-end pb-5 pt-4 px-6' },
    }"
  >
    <template #header>
      <span class="text-[21px] font-bold text-color">Details</span>
    </template>

    <template v-if="props.book">
      <p class="text-xs font-bold uppercase tracking-wide text-color">{{ props.book.title }}</p>

      <DetailField label="Author">
        <p class="text-sm font-medium text-color">{{ props.book.authorName }}</p>
      </DetailField>

      <div class="h-px w-full bg-surface-300 my-3.5" />

      <div class="flex w-full items-start gap-1.75">
        <DetailField label="Genre">
          <p class="text-sm font-medium text-color">{{ props.book.genre }}</p>
        </DetailField>
        <DetailField label="Published">
          <p class="text-sm font-medium text-color">{{ formatIsoDate(props.book.publishedDate) }}</p>
        </DetailField>
      </div>

      <div class="flex w-full items-start gap-1.75">
        <DetailField label="ISBN">
          <p class="text-sm font-medium text-color">{{ props.book.isbn }}</p>
        </DetailField>
        <DetailField label="Number of pages">
          <p class="text-sm font-medium text-color">{{ props.book.numberOfPages }}</p>
        </DetailField>
      </div>

      <div class="flex w-full items-start gap-1.75">
        <DetailField label="Price">
          <p class="text-sm font-medium text-color">{{ formatCurrency(props.book.price) }}</p>
        </DetailField>
        <DetailField label="Stock">
          <p class="text-sm font-medium text-color">{{ props.book.stock }}</p>
        </DetailField>
      </div>

      <DetailField label="Available">
        <Tag :value="props.book.isAvailable ? 'Yes' : 'No'" :severity="props.book.isAvailable ? 'success' : 'danger'" />
      </DetailField>

      <div class="h-px w-full bg-surface-300 my-3.5" />

      <DetailField label="Description">
        <p class="text-sm font-medium text-color">{{ props.book.description }}</p>
      </DetailField>
    </template>

    <template #footer>
      <Button
        label="Close"
        severity="secondary"
        outlined
        class="gap-1.75! rounded-md! border-surface-300! px-[11.5px]! py-2! text-sm! font-medium!"
        @click="emit('update:visible', false)"
      />
    </template>
  </Dialog>
</template>
