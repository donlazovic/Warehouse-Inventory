import { TextField, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { orderStatusLabels } from "../../utils/format";
import FormDialog from "../common/FormDialog";

export default function StatusChangeDialog({ request, submitting, error, onConfirm, onClose }) {
  const [note, setNote] = useState("");

  useEffect(() => {
    if (request) setNote("");
  }, [request]);

  if (!request) return null;

  const { order, status } = request;
  const isCancel = status === 6;
  const isComplete = status === 5;

  return (
    <FormDialog
      open
      title={`Promena statusa — ${order.orderNumber}`}
      submitLabel={isCancel ? "Storniraj nalog" : orderStatusLabels[status]}
      error={error}
      submitting={submitting}
      onSubmit={(event) => {
        event.preventDefault();
        onConfirm(note);
      }}
      onClose={onClose}
    >
      <Typography variant="body2">
        Nalog prelazi iz statusa <strong>{orderStatusLabels[order.status]}</strong> u{" "}
        <strong>{orderStatusLabels[status]}</strong>.
      </Typography>

      {isComplete && (
        <Typography variant="body2" sx={{ color: "warning.main" }}>
          Realizacijom se zalihe trajno menjaju. Ako nema dovoljno robe na izvornoj lokaciji,
          nalog nece biti realizovan.
        </Typography>
      )}

      {isCancel && (
        <Typography variant="body2" sx={{ color: "error.main" }}>
          Storniran nalog se ne moze vratiti u tok.
        </Typography>
      )}

      <TextField
        label="Napomena"
        value={note}
        onChange={(event) => setNote(event.target.value)}
        multiline
        minRows={2}
        fullWidth
        autoFocus
        helperText="Ostaje trajno zabelezena u istoriji naloga"
      />
    </FormDialog>
  );
}
