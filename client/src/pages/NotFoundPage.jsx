import { Box, Button, Typography } from "@mui/material";
import { Link } from "react-router-dom";

export default function NotFoundPage() {
  return (
    <Box sx={{ py: 8, maxWidth: "52ch" }}>
      <Typography variant="h1" gutterBottom>
        Stranica ne postoji
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 3 }}>
        Adresa koju ste otvorili nije deo aplikacije. Moguce je da je link zastareo.
      </Typography>
      <Button component={Link} to="/" variant="contained">
        Nazad na kontrolnu tablu
      </Button>
    </Box>
  );
}
