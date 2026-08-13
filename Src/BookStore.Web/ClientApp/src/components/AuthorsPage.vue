<script setup lang="ts">
import PageHeader from "./common/pageheader/PageHeader.vue";
import AuthorsTable from "./AuthorsTable.vue";
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
      title="Authors"
      description="Manage the catalog's authors: browse name, age, nationality, birth date and book count, and create, edit or remove entries."
    >
      <template #actions>
        <Button
          as="a"
          :href="props.createUrl"
          class="!gap-[7px] !rounded-md !border-surface-700 !bg-surface-700 !px-[11.5px] !py-2 !text-sm !font-medium hover:!bg-surface-800"
        >
          <i class="pi pi-plus text-xs" />
          New author
        </Button>
      </template>
    </PageHeader>

    <AuthorsTable
      :api-url="props.apiUrl"
      :details-url="props.detailsUrl"
      :edit-url="props.editUrl"
      :delete-url="props.deleteUrl"
    />
  </div>
</template>
