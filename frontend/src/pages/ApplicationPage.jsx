import { useCallback, useState } from "react";
import { cancelHold, confirmHold, getHold } from "../api";
import { useNow, usePolling } from "../hooks";
import { currentStatus, formatTimeLeft } from "../holds";
import { useStudent } from "../context/StudentContext";
import ErrorMessage from "../components/ErrorMessage";

export default function ApplicationPage({ holdId, onBack }) {
  const { studentId, isValid } = useStudent();
  const now = useNow();
  const [hold, setHold] = useState(null);
  const [loadError, setLoadError] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  const loadHold = useCallback(async () => {
    try {
      setHold(await getHold(holdId));
      setLoadError("");
    } catch (err) {
      setLoadError(err.message);
    }
  }, [holdId]);

  usePolling(loadHold);

  const runAction = async (action) => {
    setError("");
    setBusy(true);
    try {
      await action(holdId, studentId);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
      loadHold();
    }
  };

  const status = hold ? currentStatus(hold, now) : null;
  const isHeld = status === "HELD";

  return (
    <section>
      <button className="link" onClick={onBack}>← Back</button>
      <h2>Application</h2>
      <ErrorMessage text={loadError} />

      {hold && (
        <div className="card">
          <p>Class: {hold.className}</p>
          <p>Status: {status}</p>
          {isHeld && <p>Time remaining: {formatTimeLeft(hold, now)}</p>}

          <div className="actions">
            <button onClick={() => runAction(confirmHold)} disabled={!isHeld || !isValid || busy}>
              Pay Admission Fee
            </button>
            <button onClick={() => runAction(cancelHold)} disabled={!isHeld || !isValid || busy}>
              Cancel
            </button>
          </div>
        </div>
      )}

      <ErrorMessage text={error} />
    </section>
  );
}
