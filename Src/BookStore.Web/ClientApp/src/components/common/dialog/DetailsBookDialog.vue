<script setup lang="ts">
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import Tag from "primevue/tag";
import DetailField from "../form/DetailField.vue";
import { formatCurrency } from "../../../utils/format";
import { detailsDialogPt, dialogSecondaryButtonClass } from "./dialogStyles";
import type { BookDto } from "../../../types";

const props = defineProps<{
  visible: boolean;
  book: BookDto | null;
}>();

const emit = defineEmits<{ "update:visible": [value: boolean] }>();

// API dates are ISO strings ("yyyy-MM-ddTHH:mm:ss"); slicing avoids a local-timezone shift.
function formatIsoDate(value: string) {
  return value.slice(0, 10);
}

const dialogPt = detailsDialogPt("w-[520px]");
</script>

<template>
  <Dialog
    :visible="props.visible"
    @update:visible="(value: boolean) => emit('update:visible', value)"
    modal
    dismissable-mask
    :draggable="false"
    :pt="dialogPt"
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
        :class="dialogSecondaryButtonClass"
        @click="emit('update:visible', false)"
      />
    </template>
  </Dialog>
</template>
