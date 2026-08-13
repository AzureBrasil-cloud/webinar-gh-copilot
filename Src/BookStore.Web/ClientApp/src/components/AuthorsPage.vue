<script setup lang="ts">
import { ref } from "vue";
import PageHeader from "./common/pageheader/PageHeader.vue";
import AuthorsTable from "./AuthorsTable.vue";
import CreateAuthorDialog, { type CreateAuthorPayload } from "./common/dialog/CreateAuthorDialog.vue";
import Button from "primevue/button";
import { primaryButtonClass } from "../styles/buttonStyles";
import { useCreateEntity } from "../composables/useCreateEntity";

const props = defineProps<{
  apiUrl: string;
}>();

const authorsTable = ref<InstanceType<typeof AuthorsTable> | null>(null);

const {
  visible: createDialogVisible,
  loading: createLoading,
  error: createError,
  open: openCreate,
  submit: onCreateSubmit,
} = useCreateEntity<CreateAuthorPayload>({
  apiUrl: props.apiUrl,
  entityLabel: "author",
  onCreated: () => authorsTable.value?.reload(),
});
</script>

<template>
  <div class="flex flex-col gap-3">
    <PageHeader
      title="Authors"
      description="Manage the catalog's authors: browse name, age, nationality, birth date and book count, and create, edit or remove entries."
    >
      <template #actions>
        <Button
          :class="primaryButtonClass"
          @click="openCreate"
        >
          <i class="pi pi-plus text-xs" />
          New author
        </Button>
      </template>
    </PageHeader>

    <AuthorsTable ref="authorsTable" :api-url="props.apiUrl" />

    <CreateAuthorDialog
      v-model:visible="createDialogVisible"
      :loading="createLoading"
      :error="createError"
      @submit="onCreateSubmit"
    />
  </div>
</template>
