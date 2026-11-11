const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:8080";

const MESSAGES_BY_STATUS = {
  400: "Please check the information you entered.",
  403: "You are not allowed to do that.",
  404: "We couldn't find what you were looking for.",
  409: "That action is no longer possible. Please refresh and try again.",
};

async function request(path, { method = "GET", body, studentId } = {}) {
  const headers = { "Content-Type": "application/json" };
  if (studentId) headers["X-Student-Id"] = studentId;

  let response;
  try {
    response = await fetch(BASE_URL + path, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new Error("Cannot reach the server. Please check your connection and try again.");
  }

  if (!response.ok) throw new Error(await readErrorMessage(response));
  if (response.status === 204) return null;
  return response.json();
}

async function readErrorMessage(response) {
  const fallback =
    MESSAGES_BY_STATUS[response.status] ?? "Something went wrong on the server. Please try again.";

  try {
    const problem = await response.json();
    return problem.detail ?? fallback;
  } catch {
    return fallback;
  }
}

export const getClasses = () => request("/classes");

export const createClass = (name, totalSeats) =>
  request("/classes", { method: "POST", body: { name, totalSeats } });

export const applyForClass = (classId, studentId) =>
  request(`/classes/${classId}/holds`, { method: "POST", studentId });

export const getHold = (holdId) => request(`/holds/${holdId}`);

export const getMyHolds = (studentId) => request("/me/holds", { studentId });

export const confirmHold = (holdId, studentId) =>
  request(`/holds/${holdId}/confirm`, { method: "POST", studentId });

export const cancelHold = (holdId, studentId) =>
  request(`/holds/${holdId}`, { method: "DELETE", studentId });
