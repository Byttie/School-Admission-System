import { useStudent } from "../context/StudentContext";

export default function StudentIdBar() {
  const { studentId, setStudentId, isValid } = useStudent();

  return (
    <div className="student-bar">
      <label htmlFor="student-id">Student birth registration number</label>
      <input
        id="student-id"
        inputMode="numeric"
        value={studentId}
        onChange={(e) => setStudentId(e.target.value.trim())}
      />
      {!isValid && <span className="error">Enter a 17-digit number</span>}
    </div>
  );
}
