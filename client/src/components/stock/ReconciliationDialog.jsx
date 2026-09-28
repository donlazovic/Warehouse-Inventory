import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { useEffect, useState } from "react";
import { stockApi } from "../../api/endpoints";
import { monoFont } from "../../theme";
import { formatDateTime, formatQuantity } from "../../utils/format";

export default function ReconciliationDialog({ open, onClose }) {
  const [result, setResult] = useState(null);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  const run = () => {
    setLoading(true);
    setError(null);
    stockApi
      .reconciliation()
      .then(setResult)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    if (open) run();
  }, [open]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Provera uskladjenosti zaliha</DialogTitle>

      <DialogContent dividers>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Za svaku stavku zaliha sistem ponovo sabira sva kretanja robe od pocetka i poredi zbir sa
          upisanim stanjem. Ako se razlikuju, neko je stanje promenio mimo evidencije kretanja.
        </Typography>

        {loading && (
          <Box sx={{ display: "grid", placeItems: "center", py: 4 }}>
            <CircularProgress size={28} />
          </Box>
        )}

        {error && <Alert severity="error">{error}</Alert>}

        {result && !loading && (
          result.isConsistent ? (
            <Stack direction="row" spacing={1.5} alignItems="center" sx={{ p: 2, bgcolor: "#EDF4F1", borderRadius: 1 }}>
              <CheckCircleIcon sx={{ color: "success.main" }} />
              <Box>
                <Typography variant="body2" sx={{ fontWeight: 600 }}>
                  Sve je uskladjeno
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Provereno {result.checkedCount} stavki · {formatDateTime(result.checkedAt)}
                </Typography>
              </Box>
            </Stack>
          ) : (
            <>
              <Alert severity="error" sx={{ mb: 2 }}>
                Pronadjeno {result.mismatches.length} neslaganja od {result.checkedCount} stavki.
              </Alert>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Proizvod</TableCell>
                    <TableCell>Lokacija</TableCell>
                    <TableCell align="right">Upisano</TableCell>
                    <TableCell align="right">Po kretanjima</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {result.mismatches.map((row) => (
                    <TableRow key={`${row.productId}-${row.locationId}`}>
                      <TableCell>{row.productName}</TableCell>
                      <TableCell>{row.locationName}</TableCell>
                      <TableCell align="right" sx={{ fontFamily: monoFont }}>
                        {formatQuantity(row.recordedQuantity)}
                      </TableCell>
                      <TableCell align="right" sx={{ fontFamily: monoFont }}>
                        {formatQuantity(row.expectedQuantity)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </>
          )
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={run} disabled={loading}>
          Proveri ponovo
        </Button>
        <Button variant="contained" onClick={onClose}>
          Zatvori
        </Button>
      </DialogActions>
    </Dialog>
  );
}
