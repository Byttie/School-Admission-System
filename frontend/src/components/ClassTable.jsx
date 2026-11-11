export default function ClassTable({ classes, onApply, canApply, applyingId }) {
  return (
    <table>
      <thead>
        <tr>
          <th>Class</th>
          <th>Total</th>
          <th>On hold</th>
          <th>Confirmed</th>
          <th>Available</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {classes.map((c) => (
          <tr key={c.id}>
            <td>{c.name}</td>
            <td>{c.totalSeats}</td>
            <td>{c.heldSeats}</td>
            <td>{c.confirmedSeats}</td>
            <td>{c.availableSeats}</td>
            <td>
              <button onClick={() => onApply(c.id)} disabled={!canApply || applyingId !== null}>
                {applyingId === c.id ? "Applying…" : "Apply"}
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
