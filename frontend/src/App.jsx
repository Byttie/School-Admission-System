import { useEffect, useState } from "react";
import { api } from "./api";

export default function App() {
  const [health, setHealth] = useState("checking...");
  const [classes, setClasses] = useState([]);
  const [name, setName] = useState("");
  const [seats, setSeats] = useState(120);
  const [error, setError] = useState("");

  const load = () => api("/classes").then(setClasses).catch(e => setError(e.message));

  useEffect(() => {
    api("/health").then(h => setHealth(JSON.stringify(h))).catch(() => setHealth("backend unreachable"));
    load();
  }, []);

  const create = async () => {
    try {
      await api("/classes", { method: "POST", body: JSON.stringify({ name, totalSeats: Number(seats) }) });
      setName(""); setError(""); load();
    } catch (e) { setError(e.message); }
  };

  return (
    <div style={{ padding: 20 }}>
      <p>Health: {health}</p>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <input value={name} onChange={e => setName(e.target.value)} placeholder="Class name" />
      <input type="number" value={seats} onChange={e => setSeats(e.target.value)} />
      <button onClick={create}>Create</button>
      <ul>{classes.map(c => <li key={c.id}>{c.name} ({c.totalSeats})</li>)}</ul>
    </div>
  );
}