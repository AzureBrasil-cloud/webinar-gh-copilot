<script setup lang="ts">
import { onMounted, ref } from "vue";
import DataTable, { type DataTablePageEvent } from "primevue/datatable";
import Column from "primevue/column";
import Message from "primevue/message";
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
  <div class="rounded-border border border-surface overflow-hidden">
    <DataTable
      :value="authors"
      :loading="loading"
      lazy
      paginator
      :rows="rows"
      :totalRecords="totalRecords"
      :first="first"
      dataKey="id"
      class="text-sm"
      @page="onPage"
    >
      <Column field="name" header="Name" />
      <Column field="age" header="Age" />
      <Column field="nationality" header="Nationality" />
      <Column header="Birth date">
        <template #body="{ data }">{{ formatDate(data.birthDate) }}</template>
      </Column>
      <Column field="booksCount" header="Books" />
      <Column header="Actions" class="text-right">
        <template #body="{ data }">
          <div class="flex gap-2 justify-end">
            <a :href="`${props.detailsUrl}/${data.id}`" class="text-primary hover:underline">Details</a>
            <a :href="`${props.editUrl}/${data.id}`" class="text-primary hover:underline">Edit</a>
            <a :href="`${props.deleteUrl}/${data.id}`" class="text-red-500 hover:underline">Delete</a>
          </div>
        </template>
      </Column>
    </DataTable>
  </div>
</template>
