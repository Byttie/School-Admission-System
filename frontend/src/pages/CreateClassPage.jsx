import { useState } from "react";
import { createClass } from "../api";
import ErrorMessage from "../components/ErrorMessage";

export default function CreateClassPage() {
  const [name, setName] = useState("");
  const [seats, setSeats] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSaving(true);
    try {
      await createClass(name, Number(seats));
      setName("");
      setSeats("");
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <section>
      <h2>Create Class</h2>

      <form className="form" onSubmit={handleSubmit}>
        <label>
          Class name
          <input value={name} onChange={(e) => setName(e.target.value)} />
        </label>
        <label>
          Number of seats
          <input type="number" value={seats} onChange={(e) => setSeats(e.target.value)} />
        </label>
        <button type="submit" disabled={saving}>
          {saving ? "Creating…" : "Create class"}
        </button>
      </form>

      <ErrorMessage text={error} />
    </section>
  );
}
