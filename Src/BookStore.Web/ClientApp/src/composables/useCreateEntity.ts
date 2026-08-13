import { ref } from "vue";

interface UseCreateEntityOptions {
  /** REST endpoint to POST the new entity to, e.g. "/api/books". */
  apiUrl: string;
  /** Lower-case noun used in the fallback error message, e.g. "book". */
  entityLabel: string;
  /** Called after a successful create, typically to reload the table. */
  onCreated: () => Promise<void> | void;
}

/**
 * Shared "create entity" dialog state machine used by the entity pages
 * (BooksPage, AuthorsPage, ...). Encapsulates dialog visibility, loading/error
 * state and the POST fetch call that were previously duplicated per page.
 */
export function useCreateEntity<TPayload>(options: UseCreateEntityOptions) {
  const visible = ref(false);
  const loading = ref(false);
  const error = ref<string | null>(null);

  function open() {
    error.value = null;
    visible.value = true;
  }

  async function submit(payload: TPayload) {
    loading.value = true;
    error.value = null;
    try {
      const response = await fetch(options.apiUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });
      if (!response.ok) {
        const body = await response.json().catch(() => null);
        throw new Error(body?.message ?? `Request failed with status ${response.status}`);
      }
      visible.value = false;
      await options.onCreated();
    } catch (err) {
      error.value = err instanceof Error ? err.message : `Failed to create the ${options.entityLabel}.`;
    } finally {
      loading.value = false;
    }
  }

  return { visible, loading, error, open, submit };
}
