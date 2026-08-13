<script setup lang="ts">
import Card from "primevue/card";
import Button from "primevue/button";
import PoweredByBadge from "./PoweredByBadge.vue";

defineProps<{
  booksUrl: string;
  authorsUrl: string;
}>();
</script>

<template>
  <div class="flex flex-col gap-3 md:h-full md:min-h-0">
    <header class="shrink-0 py-4 md:py-6">
      <h1 class="text-6xl font-black leading-none text-surface-700 md:text-8xl">BookStore</h1>
      <p class="mt-3 text-lg text-surface-500 md:text-2xl">
        A sample .NET MVC project used in the GitHub Copilot webinar series.
      </p>
    </header>

    <div class="grid grid-cols-1 gap-4 md:h-full md:min-h-0 md:flex-1 md:grid-cols-2">
      <Card
        class="flex flex-col overflow-hidden !rounded-xl !shadow-sm md:h-full md:min-h-0 [&_.p-card-body]:shrink-0 md:[&_.p-card-header]:min-h-0 md:[&_.p-card-header]:flex-1"
      >
        <template #header>
          <img :src="'/images/home/books.jpg'" alt="Books" class="h-56 w-full object-cover md:h-full" />
        </template>
        <template #title>Books</template>
        <template #content>
          <p class="text-sm leading-relaxed text-surface-700">
            "Manage the bookstore catalog: browse title, author, genre, price, stock and availability, and
            create, edit or remove entries."
          </p>
        </template>
        <template #footer>
          <Button as="a" :href="booksUrl" class="w-fit !border-surface-700 !bg-surface-700 !text-white hover:!bg-surface-800">
            <i class="pi pi-book" />
            Open Books
          </Button>
        </template>
      </Card>

      <Card
        class="flex flex-col overflow-hidden !rounded-xl !shadow-sm md:h-full md:min-h-0 [&_.p-card-body]:shrink-0 md:[&_.p-card-header]:min-h-0 md:[&_.p-card-header]:flex-1"
      >
        <template #header>
          <img :src="'/images/home/authors.jpg'" alt="Authors" class="h-56 w-full object-cover md:h-full" />
        </template>
        <template #title>Authors</template>
        <template #content>
          <p class="text-sm leading-relaxed text-surface-700">
            "Manage registered authors: browse name, age, nationality, birth date and number of books, and
            create, edit or remove entries."
          </p>
        </template>
        <template #footer>
          <Button as="a" :href="authorsUrl" class="w-fit !border-surface-700 !bg-surface-700 !text-white hover:!bg-surface-800">
            <i class="pi pi-users" />
            Open Authors
          </Button>
        </template>
      </Card>
    </div>

    <div class="flex shrink-0 justify-end">
      <PoweredByBadge />
    </div>
  </div>
</template>
