import { useState } from "react";
import StudentIdBar from "./components/StudentIdBar";
import ClassesPage from "./pages/ClassesPage";
import CreateClassPage from "./pages/CreateClassPage";
import MyApplicationsPage from "./pages/MyApplicationsPage";
import ApplicationPage from "./pages/ApplicationPage";

const TABS = [
  { page: "classes", label: "Classes" },
  { page: "my-applications", label: "My Applications" },
  { page: "create-class", label: "Create Class (Admin)" },
];

export default function App() {
  const [page, setPage] = useState("classes");
  const [openHoldId, setOpenHoldId] = useState(null);

  const openApplication = (holdId) => {
    setOpenHoldId(holdId);
    setPage("application");
  };

  return (
    <div className="app">
      <header>
        <h1>School Admission</h1>
        <StudentIdBar />
        <nav>
          {TABS.map((tab) => (
            <button
              key={tab.page}
              className={page === tab.page ? "tab active" : "tab"}
              onClick={() => setPage(tab.page)}
            >
              {tab.label}
            </button>
          ))}
        </nav>
      </header>

      <main>
        {page === "classes" && <ClassesPage onApplied={openApplication} />}
        {page === "my-applications" && <MyApplicationsPage onOpen={openApplication} />}
        {page === "create-class" && <CreateClassPage />}
        {page === "application" && (
          <ApplicationPage holdId={openHoldId} onBack={() => setPage("my-applications")} />
        )}
      </main>
    </div>
  );
}
