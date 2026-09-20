import LogoutIcon from "@mui/icons-material/Logout";
import {
  AppBar,
  Box,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Tooltip,
  Typography,
} from "@mui/material";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { navigation } from "./navigation";

const DRAWER_WIDTH = 248;

export default function AppLayout() {
  const { user, signOut, can } = useAuth();
  const navigate = useNavigate();

  const handleSignOut = async () => {
    await signOut();
    navigate("/prijava", { replace: true });
  };

  return (
    <Box sx={{ display: "flex", minHeight: "100vh" }}>
      <Drawer
        variant="permanent"
        sx={{
          width: DRAWER_WIDTH,
          flexShrink: 0,
          "& .MuiDrawer-paper": {
            width: DRAWER_WIDTH,
            boxSizing: "border-box",
            bgcolor: "#101B17",
            color: "#D7DEDA",
            borderRight: "none",
          },
        }}
      >
        <Box sx={{ px: 2.5, py: 2.5 }}>
          <Typography sx={{ fontWeight: 600, fontSize: "1.05rem", color: "#FFFFFF" }}>
            Magacin
          </Typography>
          <Typography variant="body2" sx={{ color: "#7E8F87", fontSize: "0.78rem" }}>
            Upravljanje zalihama
          </Typography>
        </Box>

        <Divider sx={{ borderColor: "#1E2F28" }} />

        <Box sx={{ overflowY: "auto", py: 1 }}>
          {navigation.map((section) => {
            const visible = section.items.filter((item) => !item.permission || can(item.permission));
            if (visible.length === 0) return null;

            return (
              <Box key={section.heading} sx={{ mb: 1 }}>
                <Typography
                  variant="body2"
                  sx={{ px: 2.5, py: 1, color: "#6F8079", fontSize: "0.72rem", fontWeight: 500 }}
                >
                  {section.heading}
                </Typography>
                <List dense disablePadding>
                  {visible.map((item) => (
                    <ListItemButton
                      key={item.to}
                      component={NavLink}
                      to={item.to}
                      end={item.to === "/"}
                      sx={{
                        mx: 1,
                        borderRadius: 1,
                        color: "#B9C5BF",
                        "&.active": { bgcolor: "#1B3A2F", color: "#FFFFFF" },
                        "&:hover": { bgcolor: "#17302A" },
                      }}
                    >
                      <ListItemIcon sx={{ minWidth: 34, color: "inherit" }}>
                        <item.icon fontSize="small" />
                      </ListItemIcon>
                      <ListItemText
                        primary={item.label}
                        primaryTypographyProps={{ fontSize: "0.875rem" }}
                      />
                    </ListItemButton>
                  ))}
                </List>
              </Box>
            );
          })}
        </Box>
      </Drawer>

      <Box sx={{ flexGrow: 1, display: "flex", flexDirection: "column", minWidth: 0 }}>
        <AppBar
          position="sticky"
          elevation={0}
          sx={{ bgcolor: "background.paper", borderBottom: "1px solid", borderColor: "divider" }}
        >
          <Toolbar sx={{ justifyContent: "flex-end", gap: 2 }}>
            <Box sx={{ textAlign: "right" }}>
              <Typography variant="body2" sx={{ color: "text.primary", fontWeight: 500 }}>
                {user?.firstName} {user?.lastName}
              </Typography>
              <Typography variant="body2" sx={{ color: "text.secondary", fontSize: "0.75rem" }}>
                {user?.role}
              </Typography>
            </Box>
            <Tooltip title="Odjavi se">
              <IconButton onClick={handleSignOut} size="small">
                <LogoutIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Toolbar>
        </AppBar>

        <Box component="main" sx={{ flexGrow: 1, p: { xs: 2, md: 3 } }}>
          <Outlet />
        </Box>
      </Box>
    </Box>
  );
}
