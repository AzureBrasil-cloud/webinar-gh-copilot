<script setup lang="ts">
import { onMounted, ref } from "vue";
import { type DataTablePageEvent } from "primevue/datatable";
import Message from "primevue/message";
import DataTableCommon, { type DataTableColumn } from "./common/table/DataTableCommon.vue";
import ConfirmDeleteDialog from "./common/dialog/ConfirmDeleteDialog.vue";
import { usePagedFetch } from "../composables/usePagedFetch";
import type { AuthorDto } from "../types";

const props = defineProps<{
  apiUrl: string;
  detailsUrl: string;
  editUrl: string;
}>();

const rows = ref(10);
const first = ref(0);
const { items: authors, totalRecords, loading, error, load } = usePagedFetch<AuthorDto>(props.apiUrl);

const columns: DataTableColumn[] = [
  { field: "name", header: "Name", primary: true },
  { field: "age", header: "Age" },
  { field: "nationality", header: "Nationality" },
  { field: "birthDate", header: "Birth date" },
  { field: "booksCount", header: "Books" },
];

function onPage(event: DataTablePageEvent) {
  first.value = event.first;
  rows.value = event.rows;
  load(event.page + 1, event.rows);
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString("pt-BR");
}

const deleteDialogVisible = ref(false);
const deleteTarget = ref<AuthorDto | null>(null);
const deleteLoading = ref(false);
const deleteError = ref<string | null>(null);

function onDeleteRequest(data: AuthorDto) {
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
    if (!response.ok) {
      const body = await response.json().catch(() => null);
      throw new Error(body?.message ?? `Request failed with status ${response.status}`);
    }
    deleteDialogVisible.value = false;
    await load(Math.floor(first.value / rows.value) + 1, rows.value);
  } catch (err) {
    deleteError.value = err instanceof Error ? err.message : "Failed to delete the author.";
  } finally {
    deleteLoading.value = false;
  }
}

onMounted(() => load(1, rows.value));

defineExpose({ reload: () => load(1, rows.value) });
</script>

<template>
  <Message v-if="error" severity="error" :closable="false" class="mb-4">{{ error }}</Message>
  <DataTableCommon
    :value="authors"
    :columns="columns"
    :loading="loading"
    :total-records="totalRecords"
    :first="first"
    :rows="rows"
    :details-url="props.detailsUrl"
    :edit-url="props.editUrl"
    confirm-delete
    @page="onPage"
    @delete="onDeleteRequest"
  >
    <template #col-birthDate="{ data }">
      <span class="text-xs text-muted-color">{{ formatDate(data.birthDate) }}</span>
    </template>
  </DataTableCommon>

  <ConfirmDeleteDialog
    v-model:visible="deleteDialogVisible"
    title="Delete Author"
    message="Are you sure you want to delete this author?"
    :details="
      deleteTarget
        ? [
            { label: 'Name', value: deleteTarget.name },
            { label: 'Nationality', value: deleteTarget.nationality },
            { label: 'Books', value: String(deleteTarget.booksCount) },
          ]
        : []
    "
    :loading="deleteLoading"
    :error="deleteError"
    @confirm="onDeleteConfirm"
  />
</template>
