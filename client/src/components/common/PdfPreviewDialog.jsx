import FileDownloadOutlinedIcon from "@mui/icons-material/FileDownloadOutlined";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
} from "@mui/material";
import DialogHeader from "./DialogHeader";
import { useEffect, useRef, useState } from "react";
import { saveBlob } from "../../api/files";

export default function PdfPreviewDialog({ open, title, fileName, load, onClose }) {
  const [url, setUrl] = useState(null);
  const [blob, setBlob] = useState(null);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);
  const loadRef = useRef(load);
  loadRef.current = load;

  useEffect(() => {
    if (!open) return undefined;

    let active = true;
    let objectUrl = null;

    setLoading(true);
    setError(null);

    loadRef
      .current()
      .then((result) => {
        if (!active) return;
        objectUrl = URL.createObjectURL(result);
        setBlob(result);
        setUrl(objectUrl);
      })
      .catch((err) => active && setError(err.message))
      .finally(() => active && setLoading(false));

    return () => {
      active = false;
      if (objectUrl) URL.revokeObjectURL(objectUrl);
      setUrl(null);
      setBlob(null);
    };
  }, [open]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogHeader title={title} onClose={onClose} />
      <DialogContent dividers sx={{ p: 0, height: "78vh", bgcolor: "surface.preview" }}>
        {loading && (
          <Box sx={{ height: "100%", display: "grid", placeItems: "center" }}>
            <CircularProgress size={28} />
          </Box>
        )}
        {error && (
          <Box sx={{ p: 3 }}>
            <Alert severity="error">{error}</Alert>
          </Box>
        )}
        {url && !loading && (
          <Box component="iframe" src={url} title={title} sx={{ border: 0, width: "100%", height: "100%" }} />
        )}
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose}>Zatvori</Button>
        <Button
          variant="contained"
          startIcon={<FileDownloadOutlinedIcon />}
          disabled={!blob}
          onClick={() => saveBlob(blob, fileName)}
        >
          Preuzmi PDF
        </Button>
      </DialogActions>
    </Dialog>
  );
}
