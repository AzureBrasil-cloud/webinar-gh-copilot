<script setup lang="ts">
import { ref } from "vue";
import PageHeader from "./common/pageheader/PageHeader.vue";
import AuthorsTable from "./AuthorsTable.vue";
import CreateAuthorDialog, { type CreateAuthorPayload } from "./common/dialog/CreateAuthorDialog.vue";
import Button from "primevue/button";

const props = defineProps<{
  apiUrl: string;
  detailsUrl: string;
  editUrl: string;
}>();

const authorsTable = ref<InstanceType<typeof AuthorsTable> | null>(null);

const createDialogVisible = ref(false);
const createLoading = ref(false);
const createError = ref<string | null>(null);

function openCreate() {
  createError.value = null;
  createDialogVisible.value = true;
}

async function onCreateSubmit(payload: CreateAuthorPayload) {
  createLoading.value = true;
  createError.value = null;
  try {
    const response = await fetch(props.apiUrl, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    if (!response.ok) {
      const body = await response.json().catch(() => null);
      throw new Error(body?.message ?? `Request failed with status ${response.status}`);
    }
    createDialogVisible.value = false;
    await authorsTable.value?.reload();
  } catch (err) {
    createError.value = err instanceof Error ? err.message : "Failed to create the author.";
  } finally {
    createLoading.value = false;
  }
}
</script>

<template>
  <div class="flex flex-col gap-3">
    <PageHeader
      title="Authors"
      description="Manage the catalog's authors: browse name, age, nationality, birth date and book count, and create, edit or remove entries."
    >
      <template #actions>
        <Button
          class="gap-1.75! rounded-md! border-surface-700! bg-surface-700! px-[11.5px]! py-2! text-sm! font-medium! hover:bg-surface-800!"
          @click="openCreate"
        >
          <i class="pi pi-plus text-xs" />
          New author
        </Button>
      </template>
    </PageHeader>

    <AuthorsTable
      ref="authorsTable"
      :api-url="props.apiUrl"
      :details-url="props.detailsUrl"
      :edit-url="props.editUrl"
    />

    <CreateAuthorDialog
      v-model:visible="createDialogVisible"
      :loading="createLoading"
      :error="createError"
      @submit="onCreateSubmit"
    />
  </div>
</template>
