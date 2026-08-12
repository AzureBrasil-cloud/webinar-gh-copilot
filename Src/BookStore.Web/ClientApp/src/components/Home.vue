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
  <div class="flex flex-col gap-3">
    <header class="py-4 md:py-6">
      <h1 class="text-6xl font-black leading-none text-surface-700 md:text-8xl">BookStore</h1>
      <p class="mt-3 text-lg text-surface-500 md:text-2xl">
        A sample .NET MVC project used in the GitHub Copilot webinar series.
      </p>
    </header>

    <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
      <Card class="overflow-hidden !rounded-xl !shadow-sm">
        <template #header>
          <img :src="'/images/home/books.jpg'" alt="Books" class="h-56 w-full object-cover md:h-72" />
        </template>
        <template #title>Books</template>
        <template #content>
          <p class="text-sm leading-relaxed text-surface-700">
            "Manage the bookstore catalog: browse title, author, genre, price, stock and availability, and
            create, edit or remove entries."
          </p>
        </template>
        <template #footer>
          <Button as="a" :href="booksUrl" class="w-fit">
            <i class="pi pi-book" />
            Open Books
          </Button>
        </template>
      </Card>

      <Card class="overflow-hidden !rounded-xl !shadow-sm">
        <template #header>
          <img :src="'/images/home/authors.jpg'" alt="Authors" class="h-56 w-full object-cover md:h-72" />
        </template>
        <template #title>Authors</template>
        <template #content>
          <p class="text-sm leading-relaxed text-surface-700">
            "Manage registered authors: browse name, age, nationality, birth date and number of books, and
            create, edit or remove entries."
          </p>
        </template>
        <template #footer>
          <Button as="a" :href="authorsUrl" class="w-fit">
            <i class="pi pi-users" />
            Open Authors
          </Button>
        </template>
      </Card>
    </div>

    <div class="flex justify-end py-4">
      <PoweredByBadge />
    </div>
  </div>
</template>
