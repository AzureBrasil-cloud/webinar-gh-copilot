<script setup lang="ts">
import { onMounted, ref } from "vue";
import { type DataTablePageEvent } from "primevue/datatable";
import Tag from "primevue/tag";
import Message from "primevue/message";
import DataTableCommon, { type DataTableColumn } from "./common/table/DataTableCommon.vue";
import ConfirmDeleteDialog from "./common/dialog/ConfirmDeleteDialog.vue";
import EditBookDialog, { type EditBookPayload } from "./common/dialog/EditBookDialog.vue";
import { usePagedFetch } from "../composables/usePagedFetch";
import type { AuthorOption, BookDto } from "../types";

const props = defineProps<{
  apiUrl: string;
  detailsUrl: string;
  authors: AuthorOption[];
}>();

const rows = ref(10);
const first = ref(0);
const { items: books, totalRecords, loading, error, load } = usePagedFetch<BookDto>(props.apiUrl);

const columns: DataTableColumn[] = [
  { field: "title", header: "Title", primary: true },
  { field: "authorName", header: "Author" },
  { field: "genre", header: "Genre" },
  { field: "price", header: "Price" },
  { field: "stock", header: "Stock" },
  { field: "numberOfPages", header: "Pages" },
  { field: "isAvailable", header: "Available" },
];

function onPage(event: DataTablePageEvent) {
  first.value = event.first;
  rows.value = event.rows;
  load(event.page + 1, event.rows);
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);
}

const deleteDialogVisible = ref(false);
const deleteTarget = ref<BookDto | null>(null);
const deleteLoading = ref(false);
const deleteError = ref<string | null>(null);

function onDeleteRequest(data: BookDto) {
  deleteTarget.value = data;
  deleteError.value = null;
  deleteDialogVisible.value = true;
}

async function onDeleteConfirm() {
  if (!deleteTarget.value) return;
  deleteLoading.value = true;
  deleteError.value = null;
  try {
    const response = await fetch(`${props.apiUrl}/${deleteTarget.value.id}`, { method: "DELETE" });
    if (!response.ok) throw new Error(`Request failed with status ${response.status}`);
    deleteDialogVisible.value = false;
    await load(Math.floor(first.value / rows.value) + 1, rows.value);
  } catch (err) {
    deleteError.value = err instanceof Error ? err.message : "Failed to delete the book.";
  } finally {
    deleteLoading.value = false;
  }
}

const editDialogVisible = ref(false);
const editTarget = ref<BookDto | null>(null);
const editLoading = ref(false);
const editError = ref<string | null>(null);

function onEditRequest(data: BookDto) {
  editTarget.value = data;
  editError.value = null;
  editDialogVisible.value = true;
}

async function onEditSubmit(payload: EditBookPayload) {
  if (!editTarget.value) return;
  editLoading.value = true;
  editError.value = null;
  try {
    const response = await fetch(`${props.apiUrl}/${editTarget.value.id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    if (!response.ok) {
      const body = await response.json().catch(() => null);
      throw new Error(body?.message ?? `Request failed with status ${response.status}`);
    }
    editDialogVisible.value = false;
    await load(Math.floor(first.value / rows.value) + 1, rows.value);
  } catch (err) {
    editError.value = err instanceof Error ? err.message : "Failed to update the book.";
  } finally {
    editLoading.value = false;
  }
}

onMounted(() => load(1, rows.value));

defineExpose({ reload: () => load(1, rows.value) });
</script>


<template>
  <Message v-if="error" severity="error" :closable="false" class="mb-4">{{ error }}</Message>
  <DataTableCommon
    :value="books"
    :columns="columns"
    :loading="loading"
    :total-records="totalRecords"
    :first="first"
    :rows="rows"
    :details-url="props.detailsUrl"
    confirm-delete
    confirm-edit
    @page="onPage"
    @delete="onDeleteRequest"
    @edit="onEditRequest"
  >
    <template #col-price="{ data }">
      <span class="text-xs text-muted-color">{{ formatCurrency(data.price) }}</span>
    </template>
    <template #col-isAvailable="{ data }">
      <Tag :value="data.isAvailable ? 'Yes' : 'No'" :severity="data.isAvailable ? 'success' : 'danger'" />
    </template>
  </DataTableCommon>

  <ConfirmDeleteDialog
    v-model:visible="deleteDialogVisible"
    title="Delete Book"
    message="Are you sure you want to delete this book?"
    :details="
      deleteTarget
        ? [
            { label: 'Title', value: deleteTarget.title },
            { label: 'Author', value: deleteTarget.authorName },
            { label: 'Price', value: formatCurrency(deleteTarget.price) },
          ]
        : []
    "
    :loading="deleteLoading"
    :error="deleteError"
    @confirm="onDeleteConfirm"
  />

  <EditBookDialog
    v-model:visible="editDialogVisible"
    :book="editTarget"
    :authors="props.authors"
    :loading="editLoading"
    :error="editError"
    @submit="onEditSubmit"
  />
</template>

