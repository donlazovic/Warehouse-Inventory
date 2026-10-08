import CloseIcon from "@mui/icons-material/Close";
import SearchIcon from "@mui/icons-material/Search";
import { IconButton, InputAdornment, TextField } from "@mui/material";
import { useEffect, useRef, useState } from "react";

export default function SearchField({
  value,
  onSearch,
  delay = 300,
  ...props
}) {
  const [text, setText] = useState(value ?? "");
  const sent = useRef(value ?? "");
  const latestOnSearch = useRef(onSearch);
  latestOnSearch.current = onSearch;

  useEffect(() => {
    const external = value ?? "";
    if (external !== sent.current) {
      sent.current = external;
      setText(external);
    }
  }, [value]);

  useEffect(() => {
    if (text === sent.current) return undefined;

    const timer = setTimeout(() => {
      sent.current = text;
      latestOnSearch.current(text);
    }, delay);

    return () => clearTimeout(timer);
  }, [text, delay]);

  const clear = () => {
    setText("");
    sent.current = "";
    latestOnSearch.current("");
  };

  return (
    <TextField
      {...props}
      value={text}
      onChange={(event) => setText(event.target.value)}
      onKeyDown={(event) => {
        if (event.key === "Escape" && text) clear();
      }}
      slotProps={{
        input: {
          startAdornment: (
            <InputAdornment position="start">
              <SearchIcon fontSize="small" sx={{ color: "text.secondary" }} />
            </InputAdornment>
          ),
          endAdornment: text ? (
            <InputAdornment position="end">
              <IconButton
                size="small"
                edge="end"
                onClick={clear}
                aria-label="Obrisi pretragu"
              >
                <CloseIcon fontSize="small" />
              </IconButton>
            </InputAdornment>
          ) : null,
        },
      }}
    />
  );
}
