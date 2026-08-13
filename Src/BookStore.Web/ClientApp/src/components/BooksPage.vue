<script setup lang="ts">
import PageHeader from "./PageHeader.vue";
import BooksTable from "./BooksTable.vue";
import Button from "primevue/button";

const props = defineProps<{
  apiUrl: string;
  createUrl: string;
  detailsUrl: string;
  editUrl: string;
  deleteUrl: string;
}>();
</script>

<template>
  <div class="flex flex-col gap-3">
    <PageHeader
      title="Books"
      description='"Manage the bookstore catalog: browse title, author, genre, price, stock and availability, and create, edit or remove entries."'
    >
      <template #actions>
        <Button as="a" :href="props.createUrl" class="!bg-surface-700 !border-surface-700 hover:!bg-surface-800">
          <i class="pi pi-plus" />
          New book
        </Button>
      </template>
    </PageHeader>

    <BooksTable
      :api-url="props.apiUrl"
      :details-url="props.detailsUrl"
      :edit-url="props.editUrl"
      :delete-url="props.deleteUrl"
    />
  </div>
</template>
