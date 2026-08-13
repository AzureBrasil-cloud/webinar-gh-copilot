<script setup lang="ts">
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import Message from "primevue/message";

export interface ConfirmDeleteDetail {
  label: string;
  value: string;
}

const props = defineProps<{
  visible: boolean;
  title: string;
  message: string;
  details: ConfirmDeleteDetail[];
  loading?: boolean;
  error?: string | null;
}>();

const emit = defineEmits<{ "update:visible": [value: boolean]; confirm: []; cancel: [] }>();

function onUpdateVisible(value: boolean) {
  emit("update:visible", value);
  if (!value) emit("cancel");
}
</script>

<template>
  <Dialog
    :visible="props.visible"
    @update:visible="onUpdateVisible"
    modal
    dismissable-mask
    :closable="!loading"
    :close-on-escape="!loading"
    :draggable="false"
    :pt="{
      root: { class: 'w-[765px] max-w-[92vw] rounded-[21px] border border-surface-300 bg-surface-0 p-0 overflow-hidden' },
      header: { class: 'h-[67px] items-center justify-between pt-5 pb-4 pl-6 pr-4' },
      content: { class: 'flex flex-col gap-[21px] px-6 py-2' },
      footer: { class: 'gap-3 justify-end pb-5 pt-4 px-6' },
    }"
  >
    <template #header>
      <span class="text-[21px] font-bold text-color">{{ props.title }}</span>
    </template>

    <div class="flex w-full items-center gap-3">
      <i class="pi pi-exclamation-triangle text-base text-red-500" />
      <p class="flex-1 text-sm font-medium text-color">{{ props.message }}</p>
    </div>

    <div v-if="props.details.length" class="flex w-full flex-col gap-1.75">
      <div v-for="detail in props.details" :key="detail.label" class="flex w-full items-center gap-2 text-sm">
        <p class="shrink-0 font-semibold text-color">{{ detail.label }}:</p>
        <p class="min-w-0 flex-1 text-muted-color">{{ detail.value }}</p>
      </div>
    </div>

    <Message v-if="props.error" severity="error" :closable="false">{{ props.error }}</Message>

    <template #footer>
      <Button
        label="Cancel"
        severity="secondary"
        outlined
        :disabled="loading"
        class="gap-1.75! rounded-md! border-surface-300! px-[11.5px]! py-2! text-sm! font-medium!"
        @click="onUpdateVisible(false)"
      />
      <Button
        label="Delete"
        severity="danger"
        :loading="loading"
        class="gap-1.75! rounded-md! px-[11.5px]! py-2! text-sm! font-medium!"
        @click="emit('confirm')"
      />
    </template>
  </Dialog>
</template>
