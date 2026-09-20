import { Box, CircularProgress, Typography } from "@mui/material";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";

export default function ProtectedRoute({ permission, children }) {
  const { user, loading, can } = useAuth();
  const location = useLocation();

  if (loading) {
    return (
      <Box sx={{ display: "grid", placeItems: "center", minHeight: "100vh" }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!user) {
    return <Navigate to="/prijava" state={{ from: location }} replace />;
  }

  if (permission && !can(permission)) {
    return (
      <Box sx={{ p: 4 }}>
        <Typography variant="h2" gutterBottom>
          Nemate pristup ovoj stranici
        </Typography>
        <Typography color="text.secondary">
          Vasa uloga ({user.role}) ne obuhvata potrebnu dozvolu. Obratite se administratoru
          ako vam je pristup potreban.
        </Typography>
      </Box>
    );
  }

  return children;
}
