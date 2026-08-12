import "./style.css";
import { createApp, type Component } from "vue";
import PrimeVue from "primevue/config";
import Aura from "@primeuix/themes/aura";
import Home from "./components/Home.vue";
import BooksTable from "./components/BooksTable.vue";
import AuthorsTable from "./components/AuthorsTable.vue";
import CustomersTable from "./components/CustomersTable.vue";

function mount(component: Component, selector: string) {
  const el = document.querySelector<HTMLElement>(selector);
  if (!el) return;

  const props = { ...el.dataset };
  createApp(component, props)
    .use(PrimeVue, { theme: { preset: Aura, options: { darkModeSelector: false } } })
    .mount(el);
}

mount(Home, "#home-app");
mount(BooksTable, "#books-app");
mount(AuthorsTable, "#authors-app");
mount(CustomersTable, "#customers-app");
