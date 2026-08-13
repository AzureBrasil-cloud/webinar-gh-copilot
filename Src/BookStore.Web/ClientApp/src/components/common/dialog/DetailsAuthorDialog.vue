<script setup lang="ts">
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import DetailField from "../form/DetailField.vue";
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
</script>

<template>
  <Dialog
    :visible="visible"
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
        class="gap-1.75! rounded-md! border-surface-300! px-[11.5px]! py-2! text-sm! font-medium!"
        @click="emit('update:visible', false)"
      />
    </template>
  </Dialog>
</template>
