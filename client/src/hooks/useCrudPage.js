import { useCallback, useState } from "react";
import { useToast } from "../components/common/Toast";

export default function useCrudPage({ api, reload, labels }) {
  const toast = useToast();
  const [editing, setEditing] = useState(null);
  const [formOpen, setFormOpen] = useState(false);
  const [deleting, setDeleting] = useState(null);
  const [busy, setBusy] = useState(false);

  const openCreate = useCallback(() => {
    setEditing(null);
    setFormOpen(true);
  }, []);

  const openEdit = useCallback((row) => {
    setEditing(row);
    setFormOpen(true);
  }, []);

  const closeForm = useCallback(() => {
    setFormOpen(false);
    setEditing(null);
  }, []);

  const confirmDelete = useCallback(async () => {
    if (!deleting) return;
    setBusy(true);

    try {
      await api.remove(deleting.id);
      toast.success(labels.deleted);
      setDeleting(null);
      reload();
    } catch (err) {
      toast.error(err.message);
    } finally {
      setBusy(false);
    }
  }, [api, deleting, labels.deleted, reload, toast]);

  return {
    editing,
    formOpen,
    openCreate,
    openEdit,
    closeForm,
    deleting,
    setDeleting,
    confirmDelete,
    busy,
    toast,
  };
}
