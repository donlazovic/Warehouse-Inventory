import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";
import { tokenStore } from "../api/client";
import { authApi } from "../api/endpoints";
import { useAuth } from "../auth/AuthContext";

const HUB_URL = `${import.meta.env.VITE_API_URL ?? "https://localhost:7001"}/hubs/live`;

const EVENTS = ["ordersChanged", "stockChanged", "pendingUsersChanged", "notification"];
const RESYNC_EVENTS = ["ordersChanged", "stockChanged", "pendingUsersChanged"];

const RealtimeContext = createContext(null);

export function RealtimeProvider({ children }) {
  const { user } = useAuth();
  const [status, setStatus] = useState("offline");
  const handlers = useRef(new Map());

  const emit = useCallback((name, payload) => {
    handlers.current.get(name)?.forEach((handler) => {
      try {
        handler(payload);
      } catch (error) {
        console.error(`Real-time handler za "${name}" je pukao`, error);
      }
    });
  }, []);

  const subscribe = useCallback((name, handler) => {
    if (!handlers.current.has(name)) handlers.current.set(name, new Set());
    handlers.current.get(name).add(handler);
    return () => handlers.current.get(name)?.delete(handler);
  }, []);

  useEffect(() => {
    if (!user) return undefined;

    let stopped = false;
    let retryTimer = null;
    let wasConnected = false;

    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, { accessTokenFactory: () => tokenStore.access ?? "" })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 20000])
      .configureLogging(LogLevel.Warning)
      .build();

    EVENTS.forEach((name) => connection.on(name, (payload) => emit(name, payload)));

    const resync = () => RESYNC_EVENTS.forEach((name) => emit(name, { resync: true }));

    const start = async () => {
      if (stopped) return;
      setStatus("connecting");

      try {
        await authApi.me().catch(() => {});
        await connection.start();
        if (stopped) return;
        setStatus("online");
        if (wasConnected) resync();
        wasConnected = true;
      } catch {
        if (stopped) return;
        setStatus("offline");
        retryTimer = setTimeout(start, 10000);
      }
    };

    connection.onreconnecting(() => setStatus("reconnecting"));
    connection.onreconnected(() => {
      setStatus("online");
      resync();
    });
    connection.onclose(() => {
      if (stopped) return;
      setStatus("offline");
      retryTimer = setTimeout(start, 5000);
    });

    start();

    return () => {
      stopped = true;
      clearTimeout(retryTimer);
      connection.stop();
      setStatus("offline");
    };
  }, [user?.id, emit]);

  const value = useMemo(() => ({ status, subscribe }), [status, subscribe]);

  return <RealtimeContext.Provider value={value}>{children}</RealtimeContext.Provider>;
}

export function useRealtime() {
  const context = useContext(RealtimeContext);
  if (!context) throw new Error("useRealtime mora biti unutar RealtimeProvider-a.");
  return context;
}
