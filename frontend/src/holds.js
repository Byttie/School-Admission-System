export function currentStatus(hold, now) {
  const expired = new Date(hold.expiresAt).getTime() <= now;
  if (hold.status === "HELD" && expired) return "EXPIRED";
  return hold.status;
}

export function formatTimeLeft(hold, now) {
  const msLeft = new Date(hold.expiresAt).getTime() - now;
  if (msLeft <= 0) return "0:00";

  const totalSeconds = Math.floor(msLeft / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = String(totalSeconds % 60).padStart(2, "0");
  return `${minutes}:${seconds}`;
}
