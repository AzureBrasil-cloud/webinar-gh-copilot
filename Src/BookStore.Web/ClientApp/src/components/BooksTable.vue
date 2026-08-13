<script setup lang="ts">
import { onMounted, ref } from "vue";
import DataTable, { type DataTablePageEvent } from "primevue/datatable";
import Column from "primevue/column";
import Tag from "primevue/tag";
import Message from "primevue/message";
import Menu from "primevue/menu";
import Button from "primevue/button";
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

const menu = ref();
const menuItems = ref<Array<{ label?: string; icon?: string; class?: string; separator?: boolean; command?: () => void }>>([]);

function onPage(event: DataTablePageEvent) {
  first.value = event.first;
  rows.value = event.rows;
  load(event.page + 1, event.rows);
}

function toggleMenu(event: Event, data: BookDto) {
  menuItems.value = [
    { label: "Details", icon: "pi pi-eye", command: () => (window.location.href = `${props.detailsUrl}/${data.id}`) },
    { label: "Edit", icon: "pi pi-pencil", command: () => (window.location.href = `${props.editUrl}/${data.id}`) },
    { separator: true },
    { label: "Delete", icon: "pi pi-trash", class: "[&_.p-menu-item-link]:!text-red-500", command: () => (window.location.href = `${props.deleteUrl}/${data.id}`) },
  ];
  menu.value.toggle(event);
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);
}

onMounted(() => load(1, rows.value));
</script>

<template>
  <Message v-if="error" severity="error" :closable="false" class="mb-4">{{ error }}</Message>
  <div class="rounded-xl border border-surface-300 overflow-hidden">
    <DataTable
      :value="books"
      :loading="loading"
      lazy
      paginator
      :rows="rows"
      :rowsPerPageOptions="[10, 20, 50]"
      :totalRecords="totalRecords"
      :first="first"
      dataKey="id"
      class="text-sm"
      @page="onPage"
    >
      <Column field="title" header="Title">
        <template #body="{ data }">
          <span class="font-semibold text-color">{{ data.title }}</span>
        </template>
      </Column>
      <Column field="authorName" header="Author">
        <template #body="{ data }">
          <span class="text-xs text-muted-color">{{ data.authorName }}</span>
        </template>
      </Column>
      <Column field="genre" header="Genre">
        <template #body="{ data }">
          <span class="text-xs text-muted-color">{{ data.genre }}</span>
        </template>
      </Column>
      <Column header="Price">
        <template #body="{ data }">
          <span class="text-xs text-muted-color">{{ formatCurrency(data.price) }}</span>
        </template>
      </Column>
      <Column field="stock" header="Stock">
        <template #body="{ data }">
          <span class="text-xs text-muted-color">{{ data.stock }}</span>
        </template>
      </Column>
      <Column field="numberOfPages" header="Pages">
        <template #body="{ data }">
          <span class="text-xs text-muted-color">{{ data.numberOfPages }}</span>
        </template>
      </Column>
      <Column header="Available">
        <template #body="{ data }">
          <Tag :value="data.isAvailable ? 'Yes' : 'No'" :severity="data.isAvailable ? 'success' : 'danger'" />
        </template>
      </Column>
      <Column header="Ações" class="text-center" style="width: 4rem">
        <template #body="{ data }">
          <Button text rounded severity="secondary" aria-haspopup="true" @click="toggleMenu($event, data)">
            <i class="pi pi-ellipsis-v" />
          </Button>
        </template>
      </Column>
    </DataTable>
    <Menu ref="menu" :model="menuItems" :popup="true" />
  </div>
</template>
