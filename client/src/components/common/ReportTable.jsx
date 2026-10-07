import { useLayoutEffect, useRef, useState } from "react";
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
  const headRef = useRef(null);
  const [headHeight, setHeadHeight] = useState(0);

  useLayoutEffect(() => {
    const height = headRef.current?.offsetHeight ?? 0;
    if (height !== headHeight) setHeadHeight(height);
  });
  return (
    <Paper variant="outlined" sx={{ overflow: "hidden" }}>
      <TableContainer ref={fitRef} sx={{ maxHeight: maxHeight ?? fitHeight ?? undefined, "&::-webkit-scrollbar-track": { marginTop: `${headHeight}px` } }}>
        <Table size="small" stickyHeader>
          <TableHead ref={headRef}>
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
