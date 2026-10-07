import CloseIcon from "@mui/icons-material/Close";
import { Box, DialogTitle, IconButton, Tooltip, Typography } from "@mui/material";

export default function DialogHeader({ title, subtitle, children, onClose, disabled = false }) {
  return (
    <DialogTitle component="div" sx={{ display: "flex", alignItems: "flex-start", gap: 2 }}>
      <Box sx={{ flexGrow: 1, minWidth: 0, pt: 0.25 }}>
        {children ?? (
          <Typography component="h2" sx={{ fontSize: "1.1rem", fontWeight: 600, lineHeight: 1.4 }}>
            {title}
          </Typography>
        )}
        {subtitle && (
          <Typography variant="body2" sx={{ color: "text.secondary", mt: 0.25 }}>
            {subtitle}
          </Typography>
        )}
      </Box>

      {onClose && (
        <Tooltip title="Zatvori">
          <span>
            <IconButton
              size="small"
              onClick={onClose}
              disabled={disabled}
              aria-label="Zatvori"
              sx={{
                color: "error.main",
                border: "1px solid",
                borderColor: "transparent",
                "&:hover": { bgcolor: "tint.danger", borderColor: "error.main" },
              }}
            >
              <CloseIcon fontSize="small" />
            </IconButton>
          </span>
        </Tooltip>
      )}
    </DialogTitle>
  );
}
