<script setup lang="ts">
import { onMounted, ref } from "vue";
import { type DataTablePageEvent } from "primevue/datatable";
import Tag from "primevue/tag";
import Message from "primevue/message";
import DataTableCommon, { type DataTableColumn } from "./common/table/DataTableCommon.vue";
import ConfirmDeleteDialog from "./common/dialog/ConfirmDeleteDialog.vue";
import EditBookDialog, { type EditBookPayload } from "./common/dialog/EditBookDialog.vue";
import DetailsBookDialog from "./common/dialog/DetailsBookDialog.vue";
import { usePagedFetch } from "../composables/usePagedFetch";
import { useEntityCrud } from "../composables/useEntityCrud";
import { formatCurrency } from "../utils/format";
import type { AuthorOption, BookDto } from "../types";

const props = defineProps<{
  apiUrl: string;
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

const {
  deleteDialogVisible,
  deleteTarget,
  deleteLoading,
  deleteError,
  onDeleteRequest,
  onDeleteConfirm,
  editDialogVisible,
  editTarget,
  editLoading,
  editError,
  onEditRequest,
  onEditSubmit,
  detailsDialogVisible,
  detailsTarget,
  onDetailsRequest,
} = useEntityCrud<BookDto, EditBookPayload>({
  apiUrl: props.apiUrl,
  entityLabel: "book",
  reload: () => load(Math.floor(first.value / rows.value) + 1, rows.value),
});

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
    confirm-details
    confirm-delete
    confirm-edit
    @page="onPage"
    @delete="onDeleteRequest"
    @edit="onEditRequest"
    @details="onDetailsRequest"
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

  <DetailsBookDialog v-model:visible="detailsDialogVisible" :book="detailsTarget" />
</template>

