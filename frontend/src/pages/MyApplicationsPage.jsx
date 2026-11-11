import { useCallback, useState } from "react";
import { getMyHolds } from "../api";
import { useNow, usePolling } from "../hooks";
import { currentStatus } from "../holds";
import { useStudent } from "../context/StudentContext";
import ErrorMessage from "../components/ErrorMessage";

export default function MyApplicationsPage({ onOpen }) {
  const { studentId, isValid } = useStudent();
  const now = useNow();
  const [holds, setHolds] = useState([]);
  const [error, setError] = useState("");

  const loadHolds = useCallback(async () => {
    if (!isValid) {
      setHolds([]);
      return;
    }
    try {
      setHolds(await getMyHolds(studentId));
      setError("");
    } catch (err) {
      setError(err.message);
    }
  }, [studentId, isValid]);

  usePolling(loadHolds);

  return (
    <section>
      <h2>My Applications</h2>
      <ErrorMessage text={error} />

      <table>
        <thead>
          <tr>
            <th>Class</th>
            <th>Status</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {holds.map((hold) => (
            <tr key={hold.holdId}>
              <td>{hold.className}</td>
              <td>{currentStatus(hold, now)}</td>
              <td>
                <button onClick={() => onOpen(hold.holdId)}>Open</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
}
