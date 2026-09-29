import { useEffect, useRef } from "react";
import { useRealtime } from "../realtime/RealtimeProvider";

export default function useRealtimeEvent(name, handler) {
  const { subscribe } = useRealtime();
  const latest = useRef(handler);
  latest.current = handler;

  useEffect(() => subscribe(name, (payload) => latest.current(payload)), [name, subscribe]);
}
