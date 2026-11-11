import { createContext, useContext, useState } from "react";

const STORAGE_KEY = "studentId";
const StudentContext = createContext(null);

function loadSavedStudentId() {
  try {
    return localStorage.getItem(STORAGE_KEY) ?? "";
  } catch {
    return "";
  }
}

function saveStudentId(value) {
  try {
    localStorage.setItem(STORAGE_KEY, value);
  } catch {
    return;
  }
}

export function StudentProvider({ children }) {
  const [studentId, setStudentIdState] = useState(loadSavedStudentId);

  const setStudentId = (value) => {
    setStudentIdState(value);
    saveStudentId(value);
  };

  const isValid = /^[0-9]{17}$/.test(studentId);

  return (
    <StudentContext.Provider value={{ studentId, setStudentId, isValid }}>
      {children}
    </StudentContext.Provider>
  );
}

export function useStudent() {
  return useContext(StudentContext);
}
