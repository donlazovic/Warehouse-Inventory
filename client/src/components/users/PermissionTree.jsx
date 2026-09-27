import { Box, Checkbox, FormControlLabel, Paper, Stack, Typography } from "@mui/material";

export default function PermissionTree({ tree, selected, onChange, readOnly = false }) {
  const toggleLeaf = (id) => {
    const next = new Set(selected);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    onChange(next);
  };

  const toggleModule = (module) => {
    const childIds = module.children.map((child) => child.id);
    const allSelected = childIds.every((id) => selected.has(id));
    const next = new Set(selected);

    childIds.forEach((id) => (allSelected ? next.delete(id) : next.add(id)));
    onChange(next);
  };

  return (
    <Box
      sx={{
        display: "grid",
        gridTemplateColumns: { xs: "1fr", sm: "1fr 1fr" },
        gap: 1.5,
      }}
    >
      {tree.map((module) => {
        const childIds = module.children.map((child) => child.id);
        const selectedCount = childIds.filter((id) => selected.has(id)).length;
        const all = selectedCount === childIds.length && childIds.length > 0;
        const some = selectedCount > 0 && !all;

        return (
          <Paper key={module.id} variant="outlined" sx={{ overflow: "hidden" }}>
            <Stack
              direction="row"
              alignItems="center"
              sx={{
                px: 1,
                py: 0.25,
                bgcolor: all ? "#EDF4F1" : some ? "#F7F9FA" : "background.paper",
                borderBottom: "1px solid",
                borderColor: "divider",
              }}
            >
              <FormControlLabel
                sx={{ flexGrow: 1, mr: 0 }}
                control={
                  <Checkbox
                    size="small"
                    checked={all}
                    indeterminate={some}
                    disabled={readOnly}
                    onChange={() => toggleModule(module)}
                  />
                }
                label={
                  <Typography variant="body2" sx={{ fontWeight: 600 }}>
                    {module.name}
                  </Typography>
                }
              />
              <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.75rem", pr: 1 }}>
                {selectedCount}/{childIds.length}
              </Typography>
            </Stack>

            <Stack sx={{ px: 1, py: 0.5 }}>
              {module.children.map((child) => (
                <FormControlLabel
                  key={child.id}
                  sx={{ ml: 2.5 }}
                  control={
                    <Checkbox
                      size="small"
                      checked={selected.has(child.id)}
                      disabled={readOnly}
                      onChange={() => toggleLeaf(child.id)}
                    />
                  }
                  label={
                    <Stack direction="row" spacing={1} alignItems="baseline">
                      <Typography variant="body2">{child.name}</Typography>
                      <Typography
                        variant="body2"
                        sx={{ color: "text.disabled", fontSize: "0.7rem", fontFamily: '"IBM Plex Mono", monospace' }}
                      >
                        {child.code}
                      </Typography>
                    </Stack>
                  }
                />
              ))}
            </Stack>
          </Paper>
        );
      })}
    </Box>
  );
}
