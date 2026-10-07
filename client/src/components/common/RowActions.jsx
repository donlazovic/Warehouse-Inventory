import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import { IconButton, Stack, Tooltip } from "@mui/material";

/**
 * extra: dodatne akcije pre izmene i brisanja — [{ title, icon: Ikonica, onClick }]
 */
export default function RowActions({ onEdit, onDelete, extra = [] }) {
  return (
    <Stack
      direction="row"
      spacing={0.5}
      sx={{
        justifyContent: "flex-end",
      }}
    >
      {extra.map(({ title, icon: Icon, onClick }) => (
        <Tooltip key={title} title={title}>
          <IconButton
            size="small"
            onClick={(event) => {
              event.stopPropagation();
              onClick();
            }}
          >
            <Icon fontSize="small" />
          </IconButton>
        </Tooltip>
      ))}
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
