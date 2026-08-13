import { ref } from "vue";
import type { PagedResult } from "../types";

export function usePagedFetch<T>(apiUrl: string) {
  const items = ref<T[]>([]);
  const totalRecords = ref(0);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function load(pageNumber: number, pageSize: number) {
    loading.value = true;
    error.value = null;
    try {
      const response = await fetch(`${apiUrl}?page=${pageNumber}&pageSize=${pageSize}`);
      if (!response.ok) throw new Error(`Request failed with status ${response.status}`);
      const data: PagedResult<T> = await response.json();
      items.value = data.items;
      totalRecords.value = data.totalItems;
    } catch (err) {
      error.value = err instanceof Error ? err.message : "Failed to load data.";
    } finally {
      loading.value = false;
    }
  }

  return { items, totalRecords, loading, error, load };
}
