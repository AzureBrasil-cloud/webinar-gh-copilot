<script setup lang="ts">
import { onMounted, ref } from "vue";
import { type DataTablePageEvent } from "primevue/datatable";
import Message from "primevue/message";
import DataTableCommon, { type DataTableColumn } from "./common/table/DataTableCommon.vue";
import { usePagedFetch } from "../composables/usePagedFetch";
import type { AuthorDto } from "../types";

const props = defineProps<{
  apiUrl: string;
  detailsUrl: string;
  editUrl: string;
  deleteUrl: string;
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

onMounted(() => load(1, rows.value));
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
    :delete-url="props.deleteUrl"
    @page="onPage"
  >
    <template #col-birthDate="{ data }">
      <span class="text-xs text-muted-color">{{ formatDate(data.birthDate) }}</span>
    </template>
  </DataTableCommon>
</template>
