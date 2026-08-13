<script setup lang="ts">
import { onMounted, ref } from "vue";
import PageHeader from "./common/pageheader/PageHeader.vue";
import BooksTable from "./BooksTable.vue";
import CreateBookDialog, { type CreateBookPayload } from "./common/dialog/CreateBookDialog.vue";
import Button from "primevue/button";
import { primaryButtonClass } from "../styles/buttonStyles";
import { useCreateEntity } from "../composables/useCreateEntity";
import type { AuthorOption } from "../types";

const props = defineProps<{
  apiUrl: string;
  authorsUrl: string;
}>();

const booksTable = ref<InstanceType<typeof BooksTable> | null>(null);
const authorOptions = ref<AuthorOption[]>([]);

async function loadAuthorOptions() {
  const response = await fetch(`${props.authorsUrl}/options`);
  if (response.ok) authorOptions.value = await response.json();
}

const {
  visible: createDialogVisible,
  loading: createLoading,
  error: createError,
  open: openCreate,
  submit: onCreateSubmit,
} = useCreateEntity<CreateBookPayload>({
  apiUrl: props.apiUrl,
  entityLabel: "book",
  onCreated: () => booksTable.value?.reload(),
});

onMounted(loadAuthorOptions);
</script>


<template>
  <div class="flex flex-col gap-3">
    <PageHeader
      title="Books"
      description='"Manage the bookstore catalog: browse title, author, genre, price, stock and availability, and create, edit or remove entries."'
    >
      <template #actions>
        <Button
          :class="primaryButtonClass"
          @click="openCreate"
        >
          <i class="pi pi-plus text-xs" />
          New book
        </Button>
      </template>
    </PageHeader>

    <BooksTable ref="booksTable" :api-url="props.apiUrl" :authors="authorOptions" />

    <CreateBookDialog
      v-model:visible="createDialogVisible"
      :authors="authorOptions"
      :loading="createLoading"
      :error="createError"
      @submit="onCreateSubmit"
    />
  </div>
</template>

