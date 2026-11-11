import { useCallback, useState } from "react";
import { applyForClass, getClasses } from "../api";
import { usePolling } from "../hooks";
import { useStudent } from "../context/StudentContext";
import ClassTable from "../components/ClassTable";
import ErrorMessage from "../components/ErrorMessage";

export default function ClassesPage({ onApplied }) {
  const { studentId, isValid } = useStudent();
  const [classes, setClasses] = useState([]);
  const [loadError, setLoadError] = useState("");
  const [applyError, setApplyError] = useState("");
  const [applyingId, setApplyingId] = useState(null);

  const loadClasses = useCallback(async () => {
    try {
      setClasses(await getClasses());
      setLoadError("");
    } catch (err) {
      setLoadError(err.message);
    }
  }, []);

  usePolling(loadClasses);

  const handleApply = async (classId) => {
    setApplyError("");
    setApplyingId(classId);
    try {
      const hold = await applyForClass(classId, studentId);
      onApplied(hold.holdId);
    } catch (err) {
      setApplyError(err.message);
    } finally {
      setApplyingId(null);
    }
  };

  return (
    <section>
      <h2>Classes</h2>
      <ErrorMessage text={loadError} />
      <ErrorMessage text={applyError} />
      <ClassTable
        classes={classes}
        onApply={handleApply}
        canApply={isValid}
        applyingId={applyingId}
      />
    </section>
  );
}
