<script setup lang="ts">
import { onMounted, ref } from "vue";
import PageHeader from "./common/pageheader/PageHeader.vue";
import BooksTable from "./BooksTable.vue";
import CreateBookDialog, { type CreateBookPayload } from "./common/dialog/CreateBookDialog.vue";
import Button from "primevue/button";
import type { AuthorOption } from "../types";

const props = defineProps<{
  apiUrl: string;
  authorsUrl: string;
  detailsUrl: string;
  editUrl: string;
}>();

const booksTable = ref<InstanceType<typeof BooksTable> | null>(null);
const authorOptions = ref<AuthorOption[]>([]);

const createDialogVisible = ref(false);
const createLoading = ref(false);
const createError = ref<string | null>(null);

async function loadAuthorOptions() {
  const response = await fetch(`${props.authorsUrl}/options`);
  if (response.ok) authorOptions.value = await response.json();
}

function openCreate() {
  createError.value = null;
  createDialogVisible.value = true;
}

async function onCreateSubmit(payload: CreateBookPayload) {
  createLoading.value = true;
  createError.value = null;
  try {
    const response = await fetch(props.apiUrl, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    if (!response.ok) {
      const body = await response.json().catch(() => null);
      throw new Error(body?.message ?? `Request failed with status ${response.status}`);
    }
    createDialogVisible.value = false;
    await booksTable.value?.reload();
  } catch (err) {
    createError.value = err instanceof Error ? err.message : "Failed to create the book.";
  } finally {
    createLoading.value = false;
  }
}

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
          class="gap-1.75! rounded-md! border-surface-700! bg-surface-700! px-[11.5px]! py-2! text-sm! font-medium! hover:bg-surface-800!"
          @click="openCreate"
        >
          <i class="pi pi-plus text-xs" />
          New book
        </Button>
      </template>
    </PageHeader>

    <BooksTable
      ref="booksTable"
      :api-url="props.apiUrl"
      :details-url="props.detailsUrl"
      :edit-url="props.editUrl"
    />

    <CreateBookDialog
      v-model:visible="createDialogVisible"
      :authors="authorOptions"
      :loading="createLoading"
      :error="createError"
      @submit="onCreateSubmit"
    />
  </div>
</template>

