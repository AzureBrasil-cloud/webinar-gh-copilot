<script setup lang="ts">
import { onMounted, ref } from "vue";
import DataTable, { type DataTablePageEvent } from "primevue/datatable";
import Column from "primevue/column";
import Tag from "primevue/tag";
import Message from "primevue/message";
import { usePagedFetch } from "../composables/usePagedFetch";
import type { BookDto } from "../types";

const props = defineProps<{
  apiUrl: string;
  detailsUrl: string;
  editUrl: string;
  deleteUrl: string;
}>();

const rows = ref(10);
const first = ref(0);
const { items: books, totalRecords, loading, error, load } = usePagedFetch<BookDto>(props.apiUrl);

function onPage(event: DataTablePageEvent) {
  first.value = event.first;
  rows.value = event.rows;
  load(event.page + 1, event.rows);
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);
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
      :value="books"
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
      <Column field="title" header="Title" />
      <Column field="authorName" header="Author" />
      <Column field="genre" header="Genre" />
      <Column header="Price">
        <template #body="{ data }">{{ formatCurrency(data.price) }}</template>
      </Column>
      <Column field="stock" header="Stock" />
      <Column header="Available">
        <template #body="{ data }">
          <Tag :value="data.isAvailable ? 'Yes' : 'No'" :severity="data.isAvailable ? 'success' : 'danger'" />
        </template>
      </Column>
      <Column header="Published">
        <template #body="{ data }">{{ formatDate(data.publishedDate) }}</template>
      </Column>
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
