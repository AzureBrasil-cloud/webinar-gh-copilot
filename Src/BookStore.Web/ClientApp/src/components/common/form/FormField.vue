<script setup lang="ts">
defineProps<{
  label: string;
}>();
</script>

<template>
  <div class="flex w-full flex-1 flex-col gap-1.75">
    <label class="w-full text-sm text-color">{{ label }}</label>
    <slot />
  </div>
</template>
