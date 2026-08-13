import "./style.css";
import { createApp, type Component } from "vue";
import PrimeVue from "primevue/config";
import Aura from "@primeuix/themes/aura";
import Home from "./components/Home.vue";
import Sidebar from "./components/common/sidebar/Sidebar.vue";
import BooksPage from "./components/BooksPage.vue";
import AuthorsPage from "./components/AuthorsPage.vue";
import CustomersPage from "./components/CustomersPage.vue";

function mount(component: Component, selector: string) {
  const el = document.querySelector<HTMLElement>(selector);
  if (!el) return;

  const props = { ...el.dataset };
  createApp(component, props)
    .use(PrimeVue, { theme: { preset: Aura, options: { darkModeSelector: false } } })
    .mount(el);
}

mount(Home, "#home-app");
mount(Sidebar, "#sidebar-app");
mount(BooksPage, "#books-app");
mount(AuthorsPage, "#authors-app");
mount(CustomersPage, "#customers-app");
