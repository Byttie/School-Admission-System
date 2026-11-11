export default function ErrorMessage({ text }) {
  if (!text) return null;
  return <p className="error">{text}</p>;
}
