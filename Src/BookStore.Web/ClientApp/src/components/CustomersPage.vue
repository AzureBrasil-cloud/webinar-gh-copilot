<script setup lang="ts">
import PageHeader from "./common/pageheader/PageHeader.vue";
import CustomersTable from "./CustomersTable.vue";
import Button from "primevue/button";
import { primaryButtonClass } from "../styles/buttonStyles";

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
      title="Customers"
      description="Manage the bookstore's customers: browse full name, email, phone number and sign-up date, and create, edit or remove entries."
    >
      <template #actions>
        <Button
          as="a"
          :href="props.createUrl"
          :class="primaryButtonClass"
        >
          <i class="pi pi-plus text-xs" />
          New customer
        </Button>
      </template>
    </PageHeader>

    <CustomersTable
      :api-url="props.apiUrl"
      :details-url="props.detailsUrl"
      :edit-url="props.editUrl"
      :delete-url="props.deleteUrl"
    />
  </div>
</template>
