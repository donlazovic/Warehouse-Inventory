import { useEffect, useRef } from "react";
import { useSearchParams } from "react-router-dom";

/**
 * Cita filtere iz adrese (npr. /zalihe?categoryId=3) u oblik koji razume usePagedQuery.
 * schema: { imeParametra: "number" | "bool" | "string" }
 */
export function readUrlFilter(params, schema) {
  const filter = {};

  Object.entries(schema).forEach(([key, type]) => {
    const raw = params.get(key);
    if (raw == null || raw === "") return;

    if (type === "number") {
      const value = Number(raw);
      if (!Number.isNaN(value)) filter[key] = value;
    } else if (type === "bool") {
      filter[key] = raw === "true";
    } else {
      filter[key] = raw;
    }
  });

  return filter;
}

/**
 * Posle prvog prikaza uklanja filtere iz adrese, a ako stignu novi dok je stranica
 * vec otvorena, primenjuje ih kroz onApply.
 */
export default function useUrlFilterSync(schema, onApply) {
  const [params, setParams] = useSearchParams();
  const firstRun = useRef(true);
  const latest = useRef(onApply);
  latest.current = onApply;

  useEffect(() => {
    const keys = Object.keys(schema).filter((key) => params.has(key));
    if (keys.length === 0) {
      firstRun.current = false;
      return;
    }

    if (!firstRun.current) latest.current(readUrlFilter(params, schema));
    firstRun.current = false;

    const next = new URLSearchParams(params);
    keys.forEach((key) => next.delete(key));
    setParams(next, { replace: true });
  }, [params, schema, setParams]);
}
