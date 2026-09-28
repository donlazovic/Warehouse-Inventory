import FileDownloadOutlinedIcon from "@mui/icons-material/FileDownloadOutlined";
import PictureAsPdfOutlinedIcon from "@mui/icons-material/PictureAsPdfOutlined";
import TableViewOutlinedIcon from "@mui/icons-material/TableViewOutlined";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import { Button, Divider, ListItemIcon, ListItemText, Menu, MenuItem } from "@mui/material";
import { useState } from "react";
import { saveBlob } from "../../api/files";
import PdfPreviewDialog from "./PdfPreviewDialog";
import { useToast } from "./Toast";

export default function ExportMenu({ load, fileName, previewTitle, disabled = false }) {
  const toast = useToast();
  const [anchor, setAnchor] = useState(null);
  const [busy, setBusy] = useState(false);
  const [previewOpen, setPreviewOpen] = useState(false);

  const download = async (format) => {
    setAnchor(null);
    setBusy(true);

    try {
      const blob = await load(format);
      saveBlob(blob, `${fileName}.${format}`);
    } catch (err) {
      toast.error(err.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <>
      <Button
        variant="outlined"
        startIcon={<FileDownloadOutlinedIcon />}
        disabled={disabled || busy}
        onClick={(event) => setAnchor(event.currentTarget)}
      >
        {busy ? "Priprema..." : "Izvoz"}
      </Button>

      <Menu
        anchorEl={anchor}
        open={Boolean(anchor)}
        onClose={() => setAnchor(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        transformOrigin={{ vertical: "top", horizontal: "right" }}
      >
        <MenuItem onClick={() => download("xlsx")}>
          <ListItemIcon>
            <TableViewOutlinedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText primary="Excel" secondary=".xlsx" />
        </MenuItem>
        <MenuItem onClick={() => download("pdf")}>
          <ListItemIcon>
            <PictureAsPdfOutlinedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText primary="PDF" secondary="preuzmi" />
        </MenuItem>
        <Divider />
        <MenuItem
          onClick={() => {
            setAnchor(null);
            setPreviewOpen(true);
          }}
        >
          <ListItemIcon>
            <VisibilityOutlinedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText primary="Pregled PDF-a" />
        </MenuItem>
      </Menu>

      <PdfPreviewDialog
        open={previewOpen}
        title={previewTitle}
        fileName={`${fileName}.pdf`}
        load={() => load("pdf")}
        onClose={() => setPreviewOpen(false)}
      />
    </>
  );
}
