import LogoutIcon from "@mui/icons-material/Logout";
import NotificationsNoneIcon from "@mui/icons-material/NotificationsNone";
import SettingsOutlinedIcon from "@mui/icons-material/SettingsOutlined";
import {
  AppBar,
  Avatar,
  Badge,
  Box,
  Button,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  Popover,
  Stack,
  Toolbar,
  Tooltip,
  Typography,
} from "@mui/material";
import { useEffect, useState } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { usersApi } from "../../api/endpoints";
import { useAuth } from "../../auth/AuthContext";
import { sidebar } from "../../theme";
import Logo from "../common/Logo";
import { navigation } from "./navigation";

const DRAWER_WIDTH = 256;
const ACK_KEY = "skladisnik.ackPendingUsers";

const initialsOf = (user) =>
  `${user?.firstName?.[0] ?? ""}${user?.lastName?.[0] ?? ""}`.toUpperCase() || "?";

export default function AppLayout() {
  const { user, signOut, can } = useAuth();
  const navigate = useNavigate();

  const [userMenu, setUserMenu] = useState(null);
  const [bellAnchor, setBellAnchor] = useState(null);
  const [pendingUsers, setPendingUsers] = useState(0);

  const canSeeUsers = can("users.view");

  useEffect(() => {
    if (!canSeeUsers) return undefined;

    const load = () => usersApi.pendingCount().then(setPendingUsers).catch(() => {});
    load();
    const timer = setInterval(load, 60000);
    return () => clearInterval(timer);
  }, [canSeeUsers]);

  const [acknowledged, setAcknowledged] = useState(() => Number(localStorage.getItem(ACK_KEY) ?? 0));

  useEffect(() => {
    if (pendingUsers < acknowledged) {
      setAcknowledged(pendingUsers);
      localStorage.setItem(ACK_KEY, String(pendingUsers));
    }
  }, [pendingUsers, acknowledged]);

  const unread = Math.max(0, pendingUsers - acknowledged);

  const markAllRead = () => {
    setAcknowledged(pendingUsers);
    localStorage.setItem(ACK_KEY, String(pendingUsers));
  };

  const badges = { pendingUsers };

  const handleSignOut = async () => {
    setUserMenu(null);
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
            bgcolor: sidebar.bg,
            color: sidebar.text,
            borderRight: "none",
            display: "flex",
            flexDirection: "column",
          },
        }}
      >
        <Stack direction="row" alignItems="center" spacing={1.5} sx={{ px: 2.5, py: 2.5 }}>
          <Box sx={{ color: "#4EA88A" }}>
            <Logo size={26} />
          </Box>
          <Box>
            <Typography sx={{ fontWeight: 600, fontSize: "1.02rem", color: sidebar.textStrong, lineHeight: 1.2 }}>
              Skladisnik
            </Typography>
            <Typography sx={{ color: sidebar.textMuted, fontSize: "0.72rem" }}>
              Upravljanje zalihama
            </Typography>
          </Box>
        </Stack>

        <Divider sx={{ borderColor: sidebar.border }} />

        <Box sx={{ overflowY: "auto", py: 1, flexGrow: 1 }}>
          {navigation.map((section, index) => {
            const visible = section.items.filter((item) => !item.permission || can(item.permission));
            if (visible.length === 0) return null;

            return (
              <Box key={section.heading ?? index} sx={{ mb: 1 }}>
                {section.heading && (
                  <Typography
                    sx={{ px: 2.5, py: 1, color: sidebar.textMuted, fontSize: "0.72rem", fontWeight: 500 }}
                  >
                    {section.heading}
                  </Typography>
                )}
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
                        color: sidebar.text,
                        "&.active": { bgcolor: sidebar.bgActive, color: sidebar.textStrong },
                        "&:hover": { bgcolor: sidebar.bgHover },
                      }}
                    >
                      <ListItemIcon sx={{ minWidth: 34, color: "inherit" }}>
                        <item.icon fontSize="small" />
                      </ListItemIcon>
                      <ListItemText primary={item.label} primaryTypographyProps={{ fontSize: "0.875rem" }} />
                      {item.badgeKey && badges[item.badgeKey] > 0 && (
                        <Box
                          sx={{
                            minWidth: 20,
                            px: 0.75,
                            borderRadius: 10,
                            bgcolor: "#B4541A",
                            color: "#FFFFFF",
                            fontSize: "0.7rem",
                            fontWeight: 600,
                            textAlign: "center",
                            lineHeight: "18px",
                          }}
                        >
                          {badges[item.badgeKey]}
                        </Box>
                      )}
                    </ListItemButton>
                  ))}
                </List>
              </Box>
            );
          })}
        </Box>

        <Divider sx={{ borderColor: sidebar.border }} />

        <Stack direction="row" alignItems="center" spacing={1.5} sx={{ px: 2.5, py: 2 }}>
          <Avatar sx={{ width: 34, height: 34, bgcolor: "#2F7A61", fontSize: "0.82rem", fontWeight: 600 }}>
            {initialsOf(user)}
          </Avatar>
          <Box sx={{ minWidth: 0 }}>
            <Typography sx={{ color: sidebar.textStrong, fontSize: "0.85rem", fontWeight: 500 }} noWrap>
              {user?.firstName} {user?.lastName}
            </Typography>
            <Typography sx={{ color: sidebar.textMuted, fontSize: "0.74rem" }} noWrap>
              {user?.role}
            </Typography>
          </Box>
        </Stack>
      </Drawer>

      <Box sx={{ flexGrow: 1, display: "flex", flexDirection: "column", minWidth: 0 }}>
        <AppBar
          position="sticky"
          elevation={0}
          sx={{ bgcolor: "background.paper", borderBottom: "1px solid", borderColor: "divider" }}
        >
          <Toolbar sx={{ justifyContent: "flex-end", gap: 0.5 }}>
            <Tooltip title="Obavestenja">
              <IconButton size="small" onClick={(event) => setBellAnchor(event.currentTarget)}>
                <Badge color="warning" badgeContent={unread} invisible={unread === 0}>
                  <NotificationsNoneIcon fontSize="small" />
                </Badge>
              </IconButton>
            </Tooltip>

            <Tooltip title="Podesavanja">
              <IconButton size="small" onClick={() => navigate("/podesavanja")}>
                <SettingsOutlinedIcon fontSize="small" />
              </IconButton>
            </Tooltip>

            <IconButton size="small" sx={{ ml: 0.5 }} onClick={(event) => setUserMenu(event.currentTarget)}>
              <Avatar sx={{ width: 30, height: 30, bgcolor: "#2F7A61", fontSize: "0.75rem", fontWeight: 600 }}>
                {initialsOf(user)}
              </Avatar>
            </IconButton>
          </Toolbar>
        </AppBar>

        <Box component="main" sx={{ flexGrow: 1, p: { xs: 2, md: 3 } }}>
          <Outlet />
        </Box>
      </Box>

      <Popover
        open={Boolean(bellAnchor)}
        anchorEl={bellAnchor}
        onClose={() => setBellAnchor(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        transformOrigin={{ vertical: "top", horizontal: "right" }}
      >
        <Box sx={{ p: 2.5, width: 300 }}>
          <Typography variant="body2" sx={{ fontWeight: 600, mb: 0.5 }}>
            Obavestenja
          </Typography>
          {pendingUsers > 0 ? (
            <Box
              onClick={() => {
                setBellAnchor(null);
                navigate("/korisnici");
              }}
              sx={{
                mt: 1,
                p: 1.5,
                borderRadius: 1,
                bgcolor: unread > 0 ? "#FAF0E9" : "#F4F6F7",
                cursor: "pointer",
                "&:hover": { bgcolor: unread > 0 ? "#F5E4D7" : "#EDF0F1" },
              }}
            >
              <Typography variant="body2" sx={{ fontWeight: unread > 0 ? 500 : 400 }}>
                {pendingUsers === 1 ? "Jedan zahtev za pristup" : `${pendingUsers} zahteva za pristup`}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ fontSize: "0.78rem" }}>
                Ceka vase odobrenje
              </Typography>
            </Box>
          ) : (
            <Typography variant="body2" color="text.secondary">
              Nemate novih obavestenja.
            </Typography>
          )}

          {unread > 0 && (
            <Button size="small" fullWidth sx={{ mt: 1.5 }} onClick={markAllRead}>
              Oznaci sve kao procitano
            </Button>
          )}
        </Box>
      </Popover>

      <Menu
        anchorEl={userMenu}
        open={Boolean(userMenu)}
        onClose={() => setUserMenu(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        transformOrigin={{ vertical: "top", horizontal: "right" }}
      >
        <Box sx={{ px: 2, py: 1.5, minWidth: 220 }}>
          <Typography variant="body2" sx={{ fontWeight: 500 }}>
            {user?.firstName} {user?.lastName}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ fontSize: "0.78rem" }}>
            {user?.email}
          </Typography>
        </Box>
        <Divider />
        <MenuItem onClick={handleSignOut}>
          <ListItemIcon>
            <LogoutIcon fontSize="small" />
          </ListItemIcon>
          Odjavi se
        </MenuItem>
      </Menu>
    </Box>
  );
}
