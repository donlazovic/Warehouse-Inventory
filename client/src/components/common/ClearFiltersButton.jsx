import FilterAltOffOutlinedIcon from "@mui/icons-material/FilterAltOffOutlined";
import { Box, Button, Fade } from "@mui/material";

export default function ClearFiltersButton({ active, onClick }) {
  return (
    <>
      <Box sx={{ flexGrow: 1, display: { xs: "none", md: "block" } }} />
      <Fade in={active} unmountOnExit>
        <Button
          size="small"
          color="inherit"
          startIcon={<FilterAltOffOutlinedIcon />}
          onClick={onClick}
          sx={{ color: "text.secondary", flexShrink: 0, whiteSpace: "nowrap", "&:hover": { color: "text.primary" } }}
        >
          Ponisti filtere
        </Button>
      </Fade>
    </>
  );
}
