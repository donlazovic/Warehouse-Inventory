import KeyboardDoubleArrowLeftIcon from "@mui/icons-material/KeyboardDoubleArrowLeft";
import KeyboardDoubleArrowRightIcon from "@mui/icons-material/KeyboardDoubleArrowRight";
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
import { useCallback, useEffect, useState } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { notificationsApi, usersApi } from "../../api/endpoints";
import { useAuth } from "../../auth/AuthContext";
import useRealtimeEvent from "../../hooks/useRealtimeEvent";
import { useRealtime } from "../../realtime/RealtimeProvider";
import { useTheme } from "@mui/material/styles";
import { ColorModeToggle } from "../../ColorMode";
import Logo from "../common/Logo";
import { useToast } from "../common/Toast";
import { navigation } from "./navigation";

const DRAWER_WIDTH = 256;
const COLLAPSED_WIDTH = 68;
const COLLAPSED_KEY = "skladisnik.sidebarCollapsed";

const notificationColors = {
  1: "warning.main",
  2: "primary.main",
  3: "text.secondary",
  4: "success.main",
  5: "error.main",
  6: "warning.main",
  7: "secondary.main",
};

const connectionLabels = {
  online: { label: "Uzivo", tone: "online", hint: "Promene drugih korisnika stizu odmah." },
  connecting: { label: "Povezivanje", tone: "pending", hint: "Uspostavljanje veze sa serverom." },
  reconnecting: { label: "Ponovno povezivanje", tone: "pending", hint: "Veza je prekinuta, pokusava se ponovo." },
  offline: { label: "Van veze", tone: "offline", hint: "Podaci se ne osvezavaju sami. Proverite da li API radi." },
};

const initialsOf = (user) =>
  `${user?.firstName?.[0] ?? ""}${user?.lastName?.[0] ?? ""}`.toUpperCase() || "?";

const timeAgo = (value) => {
  const seconds = Math.max(0, (Date.now() - new Date(value).getTime()) / 1000);
  if (seconds < 60) return "upravo";
  if (seconds < 3600) return `pre ${Math.floor(seconds / 60)} min`;
  if (seconds < 86400) return `pre ${Math.floor(seconds / 3600)} h`;
  return new Date(value).toLocaleDateString("sr-Latn-RS");
};

export default function AppLayout() {
  const { user, signOut, can } = useAuth();
  const theme = useTheme();
  const sidebar = theme.palette.sidebar;
  const { status } = useRealtime();
  const toast = useToast();
  const navigate = useNavigate();

  const [collapsed, setCollapsed] = useState(() => localStorage.getItem(COLLAPSED_KEY) === "true");
  const [userMenu, setUserMenu] = useState(null);
  const [bellAnchor, setBellAnchor] = useState(null);
  const [pendingUsers, setPendingUsers] = useState(0);
  const [notifications, setNotifications] = useState([]);
  const [unread, setUnread] = useState(0);

  const canSeeUsers = can("users.view");

  const loadPending = useCallback(() => {
    if (!canSeeUsers) return;
    usersApi.pendingCount().then(setPendingUsers).catch(() => {});
  }, [canSeeUsers]);

  const loadNotifications = useCallback(() => {
    notificationsApi
      .list(20)
      .then((result) => {
        setNotifications(result.items);
        setUnread(result.unreadCount);
      })
      .catch(() => {});
  }, []);

  useEffect(() => {
    loadPending();
    loadNotifications();
  }, [loadPending, loadNotifications]);

  const toggleCollapsed = useCallback(() => {
    setCollapsed((current) => {
      localStorage.setItem(COLLAPSED_KEY, String(!current));
      return !current;
    });
  }, []);

  useEffect(() => {
    const handleKey = (event) => {
      const target = event.target;
      const typing = target.isContentEditable || ["INPUT", "TEXTAREA", "SELECT"].includes(target.tagName);
      if (typing || !(event.ctrlKey || event.metaKey) || event.key.toLowerCase() !== "b") return;
      event.preventDefault();
      toggleCollapsed();
    };

    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [toggleCollapsed]);

  useRealtimeEvent("pendingUsersChanged", loadPending);

  useRealtimeEvent("notification", (notification) => {
    setNotifications((current) => [notification, ...current].slice(0, 20));
    setUnread((current) => current + 1);
    toast.info(notification.title);
  });

  const openNotification = async (notification) => {
    setBellAnchor(null);

    if (!notification.isRead) {
      setNotifications((current) =>
        current.map((item) => (item.id === notification.id ? { ...item, isRead: true } : item))
      );
      setUnread((current) => Math.max(0, current - 1));
      notificationsApi.markRead(notification.id).catch(() => {});
    }

    if (notification.link) navigate(notification.link);
  };

  const markAllRead = async () => {
    setNotifications((current) => current.map((item) => ({ ...item, isRead: true })));
    setUnread(0);
    notificationsApi.markAllRead().catch(() => loadNotifications());
  };

  const handleSignOut = async () => {
    setUserMenu(null);
    await signOut();
    navigate("/prijava", { replace: true });
  };

  const badges = { pendingUsers };
  const drawerWidth = collapsed ? COLLAPSED_WIDTH : DRAWER_WIDTH;
  const connection = connectionLabels[status] ?? connectionLabels.offline;
  const connectionColor = theme.palette.connection[connection.tone];

  return (
    <Box sx={{ display: "flex", minHeight: "100vh" }}>
      <Drawer
        variant="permanent"
        sx={{
          width: drawerWidth,
          flexShrink: 0,
          transition: theme.transitions.create("width", { duration: 180 }),
          "& .MuiDrawer-paper": {
            width: drawerWidth,
            overflowX: "hidden",
            boxSizing: "border-box",
            bgcolor: sidebar.bg,
            color: sidebar.text,
            borderRight: "none",
            display: "flex",
            flexDirection: "column",
            transition: theme.transitions.create("width", { duration: 180 }),
          },
        }}
      >
        <Stack
          direction={collapsed ? "column" : "row"}
          sx={{
            alignItems: "center",
            gap: collapsed ? 1 : 1.5,
            px: collapsed ? 0 : 2.5,
            py: collapsed ? 1.5 : 2.25,
            minHeight: 72,
          }}
        >
          <Box sx={{ color: sidebar.logo, display: "flex" }}>
            <Logo size={26} />
          </Box>
          {!collapsed && (
            <Box sx={{ flexGrow: 1, minWidth: 0 }}>
              <Typography noWrap sx={{ fontWeight: 600, fontSize: "1.02rem", color: sidebar.textStrong, lineHeight: 1.2 }}>
                Skladisnik
              </Typography>
              <Typography noWrap sx={{ color: sidebar.textMuted, fontSize: "0.72rem" }}>
                Upravljanje zalihama
              </Typography>
            </Box>
          )}
          <Tooltip title={collapsed ? "Prosiri meni (Ctrl+B)" : "Skupi meni (Ctrl+B)"} placement="right">
            <IconButton
              size="small"
              onClick={toggleCollapsed}
              aria-label={collapsed ? "Prosiri meni" : "Skupi meni"}
              sx={{
                color: sidebar.textMuted,
                border: "1px solid",
                borderColor: sidebar.border,
                borderRadius: 1.5,
                "&:hover": { color: sidebar.textStrong, bgcolor: sidebar.bgHover },
              }}
            >
              {collapsed ? <KeyboardDoubleArrowRightIcon fontSize="small" /> : <KeyboardDoubleArrowLeftIcon fontSize="small" />}
            </IconButton>
          </Tooltip>
        </Stack>

        <Divider sx={{ borderColor: sidebar.border }} />

        <Box sx={{ overflowY: "auto", overflowX: "hidden", py: 1, flexGrow: 1 }}>
          {navigation.map((section, index) => {
            const visible = section.items.filter((item) => !item.permission || can(item.permission));
            if (visible.length === 0) return null;

            return (
              <Box key={section.heading ?? index} sx={{ mb: 1 }}>
                {section.heading &&
                  (collapsed ? (
                    <Divider sx={{ borderColor: sidebar.border, mx: 2, my: 1 }} />
                  ) : (
                    <Typography sx={{ px: 2.5, py: 1, color: sidebar.textMuted, fontSize: "0.72rem", fontWeight: 500 }}>
                      {section.heading}
                    </Typography>
                  ))}
                <List dense disablePadding>
                  {visible.map((item) => {
                    const badge = item.badgeKey ? badges[item.badgeKey] : 0;

                    const button = (
                      <ListItemButton
                        key={item.to}
                        component={NavLink}
                        to={item.to}
                        end={item.to === "/"}
                        sx={{
                          mx: 1,
                          minHeight: 40,
                          borderRadius: 1,
                          px: collapsed ? 0 : 2,
                          justifyContent: collapsed ? "center" : "flex-start",
                          color: sidebar.text,
                          "&.active": { bgcolor: sidebar.bgActive, color: sidebar.textStrong },
                          "&:hover": { bgcolor: sidebar.bgHover },
                        }}
                      >
                        <ListItemIcon sx={{ minWidth: collapsed ? 0 : 34, color: "inherit", justifyContent: "center" }}>
                          <Badge color="warning" variant="dot" invisible={!collapsed || badge === 0}>
                            <item.icon fontSize="small" />
                          </Badge>
                        </ListItemIcon>
                        {!collapsed && (
                          <ListItemText primary={item.label} primaryTypographyProps={{ fontSize: "0.875rem", noWrap: true }} />
                        )}
                        {!collapsed && badge > 0 && (
                          <Box
                            sx={{
                              minWidth: 20,
                              px: 0.75,
                              borderRadius: 10,
                              bgcolor: "warning.main",
                              color: "warning.contrastText",
                              fontSize: "0.7rem",
                              fontWeight: 600,
                              textAlign: "center",
                              lineHeight: "18px",
                            }}
                          >
                            {badge}
                          </Box>
                        )}
                      </ListItemButton>
                    );

                    return collapsed ? (
                      <Tooltip key={item.to} title={badge ? `${item.label} (${badge})` : item.label} placement="right">
                        {button}
                      </Tooltip>
                    ) : (
                      button
                    );
                  })}
                </List>
              </Box>
            );
          })}
        </Box>

        <Divider sx={{ borderColor: sidebar.border }} />

        <Tooltip title={collapsed ? `${user?.firstName ?? ""} ${user?.lastName ?? ""} · ${user?.role ?? ""}` : ""} placement="right">
          <Stack
            direction="row"
            onClick={() => navigate("/podesavanja")}
            sx={{
              alignItems: "center",
              justifyContent: collapsed ? "center" : "flex-start",
              gap: 1.5,
              px: collapsed ? 0 : 2.5,
              py: 2,
              cursor: "pointer",
              "&:hover": { bgcolor: sidebar.bgHover },
            }}
          >
            <Avatar sx={{ width: 34, height: 34, bgcolor: "avatar.strong", color: "#FFFFFF", fontSize: "0.82rem", fontWeight: 600 }}>
              {initialsOf(user)}
            </Avatar>
            {!collapsed && (
              <Box sx={{ minWidth: 0 }}>
                <Typography sx={{ color: sidebar.textStrong, fontSize: "0.85rem", fontWeight: 500 }} noWrap>
                  {user?.firstName} {user?.lastName}
                </Typography>
                <Typography sx={{ color: sidebar.textMuted, fontSize: "0.74rem" }} noWrap>
                  {user?.role}
                </Typography>
              </Box>
            )}
          </Stack>
        </Tooltip>
      </Drawer>

      <Box sx={{ flexGrow: 1, display: "flex", flexDirection: "column", minWidth: 0 }}>
        <AppBar
          position="sticky"
          elevation={0}
          sx={{ bgcolor: "background.paper", borderBottom: "1px solid", borderColor: "divider" }}
        >
          <Toolbar sx={{ justifyContent: "flex-end", gap: 0.5 }}>
            <Tooltip title={connection.hint}>
              <Stack
                direction="row"
                spacing={0.75}
                sx={{
                  alignItems: "center",
                  mr: 1.5,
                  cursor: "default"
                }}>
                <Box
                  sx={{
                    width: 8,
                    height: 8,
                    borderRadius: "50%",
                    bgcolor: connectionColor,
                    boxShadow: status === "online" ? `0 0 0 3px ${connectionColor}33` : "none",
                  }}
                />
                <Typography variant="body2" sx={{ fontSize: "0.75rem", color: "text.secondary" }}>
                  {connection.label}
                </Typography>
              </Stack>
            </Tooltip>

            <ColorModeToggle />

            <Tooltip title="Obavestenja">
              <IconButton size="small" onClick={(event) => setBellAnchor(event.currentTarget)}>
                <Badge color="warning" badgeContent={unread} invisible={unread === 0} max={99}>
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
              <Avatar sx={{ width: 30, height: 30, bgcolor: "avatar.strong", color: "#FFFFFF", fontSize: "0.75rem", fontWeight: 600 }}>
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
        <Box sx={{ width: 360 }}>
          <Stack
            direction="row"
            sx={{
              justifyContent: "space-between",
              alignItems: "center",
              px: 2,
              py: 1.5
            }}>
            <Typography variant="body2" sx={{ fontWeight: 600 }}>
              Obavestenja
            </Typography>
            {unread > 0 && (
              <Button size="small" onClick={markAllRead}>
                Oznaci sve kao procitano
              </Button>
            )}
          </Stack>
          <Divider />

          <Box sx={{ maxHeight: 420, overflowY: "auto" }}>
            {notifications.length === 0 ? (
              <Typography
                variant="body2"
                sx={{
                  color: "text.secondary",
                  p: 2.5
                }}>
                Nemate obavestenja. Ovde stizu zahtevi za odobrenje, promene statusa vasih naloga i
                upozorenja o niskim zalihama.
              </Typography>
            ) : (
              notifications.map((notification) => (
                <Stack
                  key={notification.id}
                  direction="row"
                  spacing={1.5}
                  onClick={() => openNotification(notification)}
                  sx={{
                    px: 2,
                    py: 1.5,
                    cursor: "pointer",
                    borderBottom: "1px solid",
                    borderColor: "divider",
                    bgcolor: notification.isRead ? "transparent" : "tint.unread",
                    "&:hover": { bgcolor: notification.isRead ? "tint.readHover" : "tint.unreadHover" },
                  }}
                >
                  <Box
                    sx={{
                      width: 8,
                      height: 8,
                      mt: 0.75,
                      borderRadius: "50%",
                      flexShrink: 0,
                      bgcolor: notification.isRead ? "transparent" : notificationColors[notification.type],
                      border: notification.isRead ? "1px solid" : "none",
                      borderColor: "text.disabled",
                    }}
                  />
                  <Box sx={{ minWidth: 0, flexGrow: 1 }}>
                    <Typography variant="body2" sx={{ fontWeight: notification.isRead ? 400 : 600 }}>
                      {notification.title}
                    </Typography>
                    <Typography
                      variant="body2"
                      sx={{
                        color: "text.secondary",
                        fontSize: "0.78rem"
                      }}>
                      {notification.message}
                    </Typography>
                    <Typography variant="body2" sx={{ fontSize: "0.7rem", color: "text.disabled", mt: 0.25 }}>
                      {timeAgo(notification.createdAt)}
                    </Typography>
                  </Box>
                </Stack>
              ))
            )}
          </Box>
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
          <Typography
            variant="body2"
            sx={{
              color: "text.secondary",
              fontSize: "0.78rem"
            }}>
            {user?.email}
          </Typography>
        </Box>
        <Divider />
        <MenuItem
          onClick={() => {
            setUserMenu(null);
            navigate("/podesavanja");
          }}
        >
          <ListItemIcon>
            <SettingsOutlinedIcon fontSize="small" />
          </ListItemIcon>
          Podesavanja
        </MenuItem>
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
