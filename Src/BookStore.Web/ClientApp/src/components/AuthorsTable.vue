<script setup lang="ts">
import { onMounted, ref } from "vue";
import { type DataTablePageEvent } from "primevue/datatable";
import Message from "primevue/message";
import DataTableCommon, { type DataTableColumn } from "./common/table/DataTableCommon.vue";
import ConfirmDeleteDialog from "./common/dialog/ConfirmDeleteDialog.vue";
import EditAuthorDialog, { type EditAuthorPayload } from "./common/dialog/EditAuthorDialog.vue";
import DetailsAuthorDialog from "./common/dialog/DetailsAuthorDialog.vue";
import { usePagedFetch } from "../composables/usePagedFetch";
import { useEntityCrud } from "../composables/useEntityCrud";
import { formatDate } from "../utils/format";
import type { AuthorDto } from "../types";

const props = defineProps<{
  apiUrl: string;
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
} = useEntityCrud<AuthorDto, EditAuthorPayload>({
  apiUrl: props.apiUrl,
  entityLabel: "author",
  reload: () => load(Math.floor(first.value / rows.value) + 1, rows.value),
});

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
    confirm-details
    confirm-delete
    confirm-edit
    @page="onPage"
    @delete="onDeleteRequest"
    @edit="onEditRequest"
    @details="onDetailsRequest"
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

  <EditAuthorDialog
    v-model:visible="editDialogVisible"
    :author="editTarget"
    :loading="editLoading"
    :error="editError"
    @submit="onEditSubmit"
  />

  <DetailsAuthorDialog v-model:visible="detailsDialogVisible" :author="detailsTarget" />
</template>
