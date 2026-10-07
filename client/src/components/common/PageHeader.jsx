import { Box, Stack, Typography } from "@mui/material";
import { useEffect } from "react";

export default function PageHeader({ title, description, actions, documentTitle }) {
  const tabTitle = documentTitle ?? title;

  useEffect(() => {
    document.title = tabTitle ? `${tabTitle} · Skladisnik` : "Skladisnik";
  }, [tabTitle]);

  return (
    <Stack
      direction={{ xs: "column", sm: "row" }}
      spacing={2}
      sx={{
        justifyContent: "space-between",
        alignItems: { xs: "stretch", sm: "flex-start" },
        mb: 3
      }}>
      <Box>
        <Typography variant="h1">{title}</Typography>
        {description && (
          <Typography
            variant="body2"
            sx={{
              color: "text.secondary",
              mt: 0.5,
              maxWidth: "62ch"
            }}>
            {description}
          </Typography>
        )}
      </Box>
      {actions && <Stack direction="row" spacing={1}>{actions}</Stack>}
    </Stack>
  );
}
