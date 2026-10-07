import { useLayoutEffect, useRef, useState } from "react";
import useFitHeight from "../../hooks/useFitHeight";
import {
  Box,
  CircularProgress,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TableSortLabel,
  Typography,
} from "@mui/material";

export default function DataTable({
  columns,
  rows,
  loading = false,
  totalCount = 0,
  page = 1,
  pageSize = 20,
  sortBy,
  sortDesc = false,
  onPageChange,
  onPageSizeChange,
  onSortChange,
  onRowClick,
  emptyTitle = "Nema podataka",
  emptyHint,
}) {
  const [fitRef, fitHeight] = useFitHeight({ reserve: 88 });
  const headRef = useRef(null);
  const [headHeight, setHeadHeight] = useState(0);

  useLayoutEffect(() => {
    const height = headRef.current?.offsetHeight ?? 0;
    if (height !== headHeight) setHeadHeight(height);
  });

  const handleSort = (field) => {
    if (!onSortChange) return;
    onSortChange(field, sortBy === field ? !sortDesc : false);
  };

  return (
    <Paper variant="outlined" sx={{ overflow: "hidden" }}>
      <TableContainer ref={fitRef} sx={{ position: "relative", maxHeight: fitHeight ?? undefined, "&::-webkit-scrollbar-track": { marginTop: `${headHeight}px` } }}>
        {loading && (
          <Box
            sx={{
              position: "absolute",
              inset: 0,
              display: "grid",
              placeItems: "center",
              bgcolor: "surface.overlay",
              zIndex: 2,
            }}
          >
            <CircularProgress size={28} />
          </Box>
        )}

        <Table size="small" stickyHeader>
          <TableHead ref={headRef}>
            <TableRow>
              {columns.map((column) => (
                <TableCell
                  key={column.field}
                  align={column.align ?? "left"}
                  sx={{ width: column.width }}
                >
                  {column.sortable ? (
                    <TableSortLabel
                      active={sortBy === column.field}
                      direction={sortBy === column.field && sortDesc ? "desc" : "asc"}
                      onClick={() => handleSort(column.field)}
                    >
                      {column.headerName}
                    </TableSortLabel>
                  ) : (
                    column.headerName
                  )}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>

          <TableBody>
            {rows.length === 0 && !loading ? (
              <TableRow>
                <TableCell colSpan={columns.length} sx={{ py: 6, textAlign: "center" }}>
                  <Typography variant="h3" gutterBottom>
                    {emptyTitle}
                  </Typography>
                  {emptyHint && (
                    <Typography variant="body2" sx={{
                      color: "text.secondary"
                    }}>
                      {emptyHint}
                    </Typography>
                  )}
                </TableCell>
              </TableRow>
            ) : (
              rows.map((row) => (
                <TableRow
                  key={row.id}
                  hover={Boolean(onRowClick)}
                  onClick={() => onRowClick?.(row)}
                  sx={{ cursor: onRowClick ? "pointer" : "default" }}
                >
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

      <TablePagination
        component="div"
        count={totalCount}
        page={Math.max(0, page - 1)}
        rowsPerPage={pageSize}
        rowsPerPageOptions={[10, 20, 50, 100]}
        onPageChange={(_, nextPage) => onPageChange?.(nextPage + 1)}
        onRowsPerPageChange={(event) => onPageSizeChange?.(Number(event.target.value))}
        labelRowsPerPage="Redova po strani"
        labelDisplayedRows={({ from, to, count }) => `${from}–${to} od ${count}`}
      />
    </Paper>
  );
}
