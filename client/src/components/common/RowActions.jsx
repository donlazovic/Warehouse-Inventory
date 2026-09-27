import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import { IconButton, Stack, Tooltip } from "@mui/material";

export default function RowActions({ onEdit, onDelete }) {
  return (
    <Stack direction="row" spacing={0.5} justifyContent="flex-end">
      {onEdit && (
        <Tooltip title="Izmeni">
          <IconButton
            size="small"
            onClick={(event) => {
              event.stopPropagation();
              onEdit();
            }}
          >
            <EditOutlinedIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      )}
      {onDelete && (
        <Tooltip title="Obrisi">
          <IconButton
            size="small"
            onClick={(event) => {
              event.stopPropagation();
              onDelete();
            }}
          >
            <DeleteOutlinedIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      )}
    </Stack>
  );
}
