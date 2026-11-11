import { useEffect, useState } from "react";

export const POLL_INTERVAL_MS = 3000;

export function usePolling(load, intervalMs = POLL_INTERVAL_MS) {
  useEffect(() => {
    load();
    const timer = setInterval(load, intervalMs);
    return () => clearInterval(timer);
  }, [load, intervalMs]);
}

export function useNow() {
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    const timer = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(timer);
  }, []);

  return now;
}
