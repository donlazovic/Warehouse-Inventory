import KeyboardCapslockIcon from "@mui/icons-material/KeyboardCapslock";
import VisibilityOffOutlinedIcon from "@mui/icons-material/VisibilityOffOutlined";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import { IconButton, InputAdornment, Stack, TextField, Tooltip } from "@mui/material";
import { useState } from "react";

export default function PasswordField({ helperText, error, onKeyDown, onKeyUp, onBlur, ...props }) {
  const [visible, setVisible] = useState(false);
  const [capsLock, setCapsLock] = useState(false);

  const detect = (event) => {
    if (typeof event.getModifierState === "function") setCapsLock(event.getModifierState("CapsLock"));
  };

  const showCapsWarning = capsLock && !error;

  return (
    <TextField
      {...props}
      error={error}
      type={visible ? "text" : "password"}
      onKeyDown={(event) => {
        detect(event);
        onKeyDown?.(event);
      }}
      onKeyUp={(event) => {
        detect(event);
        onKeyUp?.(event);
      }}
      onBlur={(event) => {
        setCapsLock(false);
        onBlur?.(event);
      }}
      helperText={showCapsWarning ? "Caps Lock je ukljucen" : helperText}
      slotProps={{
        formHelperText: showCapsWarning ? { sx: { color: "warning.main", fontWeight: 500 } } : undefined,
        input: {
          endAdornment: (
            <InputAdornment position="end">
              <Stack direction="row" sx={{ alignItems: "center", gap: 0.25 }}>
                {capsLock && (
                  <Tooltip title="Caps Lock je ukljucen">
                    <KeyboardCapslockIcon fontSize="small" sx={{ color: "warning.main" }} />
                  </Tooltip>
                )}
                <IconButton
                  size="small"
                  edge="end"
                  onClick={() => setVisible((current) => !current)}
                  aria-label={visible ? "Sakrij lozinku" : "Prikazi lozinku"}
                >
                  {visible ? <VisibilityOffOutlinedIcon fontSize="small" /> : <VisibilityOutlinedIcon fontSize="small" />}
                </IconButton>
              </Stack>
            </InputAdornment>
          ),
        },
      }}
    />
  );
}
