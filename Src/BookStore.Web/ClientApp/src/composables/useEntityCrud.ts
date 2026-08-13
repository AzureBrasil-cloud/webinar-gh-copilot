import { ref } from "vue";

interface UseEntityCrudOptions {
  /** Base REST endpoint for the entity, e.g. "/api/books". */
  apiUrl: string;
  /** Lower-case noun used in fallback error messages, e.g. "book". */
  entityLabel: string;
  /** Reloads the current page of the table after a successful delete/edit. */
  reload: () => Promise<void> | void;
}

async function parseErrorMessage(response: Response): Promise<string> {
  const body = await response.json().catch(() => null);
  return body?.message ?? `Request failed with status ${response.status}`;
}

/**
 * Shared delete/edit/details row-action state machine used by the entity DataTables
 * (BooksTable, AuthorsTable, ...). Encapsulates the dialog visibility, target row,
 * loading/error state and fetch calls that were previously duplicated per table.
 */
export function useEntityCrud<TEntity extends { id: number }, TEditPayload>(
  options: UseEntityCrudOptions,
) {
  const { apiUrl, entityLabel, reload } = options;

  const deleteDialogVisible = ref(false);
  const deleteTarget = ref<TEntity | null>(null);
  const deleteLoading = ref(false);
  const deleteError = ref<string | null>(null);

  function onDeleteRequest(data: TEntity) {
    deleteTarget.value = data;
    deleteError.value = null;
    deleteDialogVisible.value = true;
  }

  async function onDeleteConfirm() {
    if (!deleteTarget.value) return;
    deleteLoading.value = true;
    deleteError.value = null;
    try {
      const response = await fetch(`${apiUrl}/${deleteTarget.value.id}`, { method: "DELETE" });
      if (!response.ok) throw new Error(await parseErrorMessage(response));
      deleteDialogVisible.value = false;
      await reload();
    } catch (err) {
      deleteError.value = err instanceof Error ? err.message : `Failed to delete the ${entityLabel}.`;
    } finally {
      deleteLoading.value = false;
    }
  }

  const editDialogVisible = ref(false);
  const editTarget = ref<TEntity | null>(null);
  const editLoading = ref(false);
  const editError = ref<string | null>(null);

  function onEditRequest(data: TEntity) {
    editTarget.value = data;
    editError.value = null;
    editDialogVisible.value = true;
  }

  async function onEditSubmit(payload: TEditPayload) {
    if (!editTarget.value) return;
    editLoading.value = true;
    editError.value = null;
    try {
      const response = await fetch(`${apiUrl}/${editTarget.value.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });
      if (!response.ok) throw new Error(await parseErrorMessage(response));
      editDialogVisible.value = false;
      await reload();
    } catch (err) {
      editError.value = err instanceof Error ? err.message : `Failed to update the ${entityLabel}.`;
    } finally {
      editLoading.value = false;
    }
  }

  const detailsDialogVisible = ref(false);
  const detailsTarget = ref<TEntity | null>(null);

  function onDetailsRequest(data: TEntity) {
    detailsTarget.value = data;
    detailsDialogVisible.value = true;
  }

  return {
    deleteDialogVisible,
    deleteTarget,
    deleteLoading,
    deleteError,
    onDeleteRequest,
    onDeleteConfirm,
    editDialogVisible,
    editTarget,
    editLoading,
    editError,
    onEditRequest,
    onEditSubmit,
    detailsDialogVisible,
    detailsTarget,
    onDetailsRequest,
  };
}
