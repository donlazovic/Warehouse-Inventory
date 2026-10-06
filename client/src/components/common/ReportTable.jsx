import useFitHeight from "../../hooks/useFitHeight";
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";

export default function ReportTable({ columns, rows, rowKey, emptyText = "Nema podataka za izabrane filtere.", maxHeight, reserve = 72 }) {
  const [fitRef, fitHeight] = useFitHeight({ reserve });
  return (
    <Paper variant="outlined">
      <TableContainer ref={fitRef} sx={{ maxHeight: maxHeight ?? fitHeight ?? undefined }}>
        <Table size="small" stickyHeader>
          <TableHead>
            <TableRow>
              {columns.map((column) => (
                <TableCell key={column.field} align={column.align ?? "left"}>
                  {column.headerName}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} sx={{ py: 5, textAlign: "center" }}>
                  <Typography variant="body2" sx={{
                    color: "text.secondary"
                  }}>
                    {emptyText}
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              rows.map((row) => (
                <TableRow key={rowKey(row)} hover>
                  {columns.map((column) => (
                    <TableCell key={column.field} align={column.align ?? "left"}>
                      {column.render ? column.render(row) : row[column.field]}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  );
}
