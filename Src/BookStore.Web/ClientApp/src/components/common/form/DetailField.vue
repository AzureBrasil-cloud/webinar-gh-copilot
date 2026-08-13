<script setup lang="ts">
defineProps<{
  label: string;
}>();
</script>

<template>
  <div class="flex min-w-0 flex-1 flex-col items-start gap-1.75">
    <p class="text-xs font-bold uppercase tracking-wide text-color">{{ label }}</p>
    <slot />
  </div>
</template>
