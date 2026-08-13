<script setup lang="ts">
import { ref } from "vue";
import DataTable, { type DataTablePageEvent } from "primevue/datatable";
import Column from "primevue/column";
import Menu from "primevue/menu";
import Button from "primevue/button";

export interface DataTableColumn {
  field: string;
  header: string;
  /** Emphasized styling for the row's primary/identifying column (e.g. Title, Name). */
  primary?: boolean;
}

type MenuItem = { label?: string; icon?: string; command?: () => void };

// Matches Figma "modal action" popup (node 39:22030): white panel, 12px radius, 4/8px list padding, 2px item gap.
const menuPt = {
  root: { class: "min-w-[140px] rounded-xl border border-surface-300 bg-surface-0 p-0" },
  list: { class: "flex flex-col gap-0.5 px-1 py-2" },
  itemLink: { class: "gap-1.75 rounded-md px-[10.5px] py-[7px] text-sm font-normal text-color" },
  itemIcon: { class: "text-sm" },
};

// Matches Figma "paginator" component (node 8:7279): flat, borderless nav buttons, no filled
// highlight on the active page (just bolder text), plain-bordered rows-per-page select.
// `!` overrides are required because PrimeVue's runtime style tag is injected after Tailwind's,
// so equal-specificity utility classes would otherwise lose to the theme's default page/button colors.
const navButtonPt = { class: "h-[35px] w-[35px] rounded-full bg-transparent! text-muted-color hover:bg-surface-100! hover:text-color!" };
const paginatorPt = {
  root: { class: "justify-end gap-[3.5px] rounded-none bg-transparent! px-3.5 py-1.75" },
  first: navButtonPt,
  prev: navButtonPt,
  next: navButtonPt,
  last: navButtonPt,
  page: ({ context }: { context: { active: boolean } }) => ({
    class: [
      "h-[35px] w-[35px] rounded-full bg-transparent! text-sm hover:bg-surface-100!",
      context.active ? "font-semibold text-color!" : "font-normal text-muted-color!",
    ],
  }),
  pcRowPerPageDropdown: {
    root: { class: "rounded-md border border-surface-300 bg-surface-0" },
  },
};

const props = withDefaults(
  defineProps<{
    value: any[];
    columns: DataTableColumn[];
    loading?: boolean;
    totalRecords?: number;
    first?: number;
    rows?: number;
    rowsPerPageOptions?: number[];
    dataKey?: string;
    detailsUrl?: string;
    editUrl?: string;
    deleteUrl?: string;
    /** When true, the Delete action emits a `delete` event instead of navigating to deleteUrl. */
    confirmDelete?: boolean;
    /** When true, the Edit action emits an `edit` event instead of navigating to editUrl. */
    confirmEdit?: boolean;
    /** When true, the Details action emits a `details` event instead of navigating to detailsUrl. */
    confirmDetails?: boolean;
  }>(),
  {
    rowsPerPageOptions: () => [10, 20, 50],
    dataKey: "id",
    rows: 10,
    first: 0,
  },
);

const emit = defineEmits<{ page: [event: DataTablePageEvent]; delete: [data: any]; edit: [data: any]; details: [data: any] }>();

const hasActions = !!(
  props.detailsUrl ||
  props.confirmDetails ||
  props.editUrl ||
  props.confirmEdit ||
  props.deleteUrl ||
  props.confirmDelete
);

const menu = ref();
const menuItems = ref<MenuItem[]>([]);

function toggleMenu(event: Event, data: any) {
  menuItems.value = [
    ...(props.detailsUrl || props.confirmDetails
      ? [
          {
            label: "Details",
            icon: "pi pi-eye",
            command: () => {
              if (props.confirmDetails) emit("details", data);
              else window.location.href = `${props.detailsUrl}/${data.id}`;
            },
          },
        ]
      : []),
    ...(props.editUrl || props.confirmEdit
      ? [
          {
            label: "Edit",
            icon: "pi pi-pencil",
            command: () => {
              if (props.confirmEdit) emit("edit", data);
              else window.location.href = `${props.editUrl}/${data.id}`;
            },
          },
        ]
      : []),
    ...(props.deleteUrl || props.confirmDelete
      ? [
          {
            label: "Delete",
            icon: "pi pi-trash",
            command: () => {
              if (props.confirmDelete) emit("delete", data);
              else window.location.href = `${props.deleteUrl}/${data.id}`;
            },
          },
        ]
      : []),
  ];
  menu.value.toggle(event);
}
</script>

<template>
  <div class="rounded-xl border border-surface-300 overflow-hidden">
    <DataTable
      :value="value"
      :loading="loading"
      lazy
      paginator
      row-hover
      :rows="rows"
      :rowsPerPageOptions="rowsPerPageOptions"
      :totalRecords="totalRecords"
      :first="first"
      :dataKey="dataKey"
      class="text-sm"
      :pt="{ pcPaginator: paginatorPt }"
      @page="(e) => $emit('page', e)"
    >
      <Column v-for="col in columns" :key="col.field" :field="col.field" :header="col.header">
        <template #body="{ data }">
          <slot :name="`col-${col.field}`" :data="data">
            <span :class="col.primary ? 'font-semibold text-color' : 'text-xs text-muted-color'">{{ data[col.field] }}</span>
          </slot>
        </template>
      </Column>
      <Column v-if="hasActions" header="Actions" class="text-center" style="width: 4rem">
        <template #body="{ data }">
          <Button text rounded severity="secondary" aria-haspopup="true" @click="toggleMenu($event, data)">
            <i class="pi pi-ellipsis-v" />
          </Button>
        </template>
      </Column>
    </DataTable>
    <Menu ref="menu" :model="menuItems" :popup="true" :pt="menuPt" />
  </div>
</template>
