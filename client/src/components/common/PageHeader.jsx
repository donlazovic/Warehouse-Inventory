import { Box, Stack, Typography } from "@mui/material";

export default function PageHeader({ title, description, actions }) {
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
