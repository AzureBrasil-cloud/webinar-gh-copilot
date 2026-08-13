<script setup lang="ts">
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import DetailField from "../form/DetailField.vue";
import { detailsDialogPt, dialogSecondaryButtonClass } from "./dialogStyles";
import type { AuthorDto } from "../../../types";

defineProps<{
  visible: boolean;
  author: AuthorDto | null;
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
    :visible="visible"
    @update:visible="(value: boolean) => emit('update:visible', value)"
    modal
    dismissable-mask
    :draggable="false"
    :pt="dialogPt"
  >
    <template #header>
      <span class="text-[21px] font-bold text-color">Details</span>
    </template>

    <template v-if="author">
      <!-- Figma reuses the Book-details "Author" caption here; relabeled to "Name" since this modal IS the author's own record. -->
      <DetailField label="Name">
        <p class="text-sm font-medium text-color">{{ author.name }}</p>
      </DetailField>

      <div class="h-px w-full bg-surface-300 my-3.5" />

      <div class="flex w-full items-start gap-1.75">
        <DetailField label="Nationality">
          <p class="text-sm font-medium text-color">{{ author.nationality }}</p>
        </DetailField>
        <DetailField label="Age">
          <p class="text-sm font-medium text-color">{{ author.age }}</p>
        </DetailField>
      </div>

      <div class="flex w-full items-start gap-1.75">
        <DetailField label="Birth date">
          <p class="text-sm font-medium text-color">{{ formatIsoDate(author.birthDate) }}</p>
        </DetailField>
        <!-- Figma shows "Number of pages" here (leftover from the Book-details layout); Authors have no page count, so this shows the book count instead. -->
        <DetailField label="Number of books">
          <p class="text-sm font-medium text-color">{{ author.booksCount }}</p>
        </DetailField>
      </div>

      <DetailField label="Books">
        <ul v-if="author.books.length" class="ml-[21px] list-disc text-sm font-medium text-color">
          <li v-for="book in author.books" :key="book.title">
            {{ book.title }} <span class="text-muted-color">({{ book.publishedYear }})</span>
          </li>
        </ul>
        <p v-else class="text-sm font-medium text-muted-color">No books yet.</p>
      </DetailField>

      <div class="h-px w-full bg-surface-300 my-3.5" />

      <DetailField label="Bio">
        <p class="text-sm font-medium text-color">{{ author.bio }}</p>
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
