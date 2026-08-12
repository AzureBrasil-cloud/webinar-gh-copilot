<script setup lang="ts">
import { computed } from "vue";
import Avatar from "primevue/avatar";

const props = defineProps<{
  active: string;
  homeUrl: string;
  booksUrl: string;
  authorsUrl: string;
  customersUrl: string;
}>();

const navItems = computed(() => [
  { key: "Home", label: "Home", icon: "pi-home", href: props.homeUrl },
  { key: "Books", label: "Books", icon: "pi-book", href: props.booksUrl },
  { key: "Authors", label: "Authors", icon: "pi-users", href: props.authorsUrl },
  { key: "Customers", label: "Customers", icon: "pi-address-book", href: props.customersUrl },
]);
</script>

<template>
  <aside
    class="sticky top-4 flex h-[calc(100vh-2rem)] w-69.5 shrink-0 flex-col gap-1.75 rounded-xl border border-surface-300 bg-surface-0 px-[17.5px] py-[17.5px]"
  >
    <div class="flex w-full items-center gap-1.75 py-[17.5px]">
      <div class="flex size-10 shrink-0 items-center justify-center rounded-lg border border-surface-300 bg-surface-50">
        <img :src="'/images/home/books.jpg'" alt="" class="size-6 rounded-md object-cover" />
      </div>
      <div class="flex min-w-0 flex-1 flex-col">
        <p class="truncate text-[22px] font-black leading-none text-surface-700">BookStore</p>
        <p class="text-xs text-muted-color">Premium catalog</p>
      </div>
    </div>

    <div class="h-px w-full bg-surface-300"></div>

    <nav class="flex w-full flex-col gap-1.75 py-[17.5px]">
      <a
        v-for="item in navItems"
        :key="item.key"
        :href="item.href"
        class="flex w-full items-center gap-1.75 rounded-lg border border-surface-300 p-[11.5px] text-sm font-medium no-underline text-color"
        :class="item.key === active ? 'bg-blue-100' : 'bg-surface-0 hover:bg-surface-50'"
      >
        <i class="pi text-sm" :class="item.icon" />
        <span>{{ item.label }}</span>
      </a>
    </nav>

    <div class="flex-1"></div>

    <div class="flex w-full items-center gap-1.75 rounded-xl border border-surface-300 bg-surface-50 p-[17.5px]">
      <Avatar icon="pi pi-user" shape="circle" class="bg-surface-300! text-surface-700!" />
      <div class="flex min-w-0 flex-1 flex-col">
        <p class="truncate text-sm font-semibold text-color">Felipe Faria</p>
        <p class="truncate text-xs text-muted-color">felipe@email.com</p>
      </div>
    </div>
  </aside>
</template>
