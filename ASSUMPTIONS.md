# Assumptions

- **Class names** are free text; blank names are rejected and duplicates are allowed. Seats must be 1–10,000.
- **Student ID** (`X-Student-Id`) must be exactly 17 digits, otherwise `400`. With no login, the ID in the header is trusted. `GET /holds/{holdId}` does not require it, as the contract does not list it.
- **Multiple classes:** a student may apply to different classes at the same time. Only one active application (held and not expired, or confirmed) per class is allowed; after expiry or cancellation they may apply again.
- **`HOLD_DURATION_SECONDS`** is read at startup and only affects new holds.
- **Repeated requests:** confirming twice returns `200`; cancelling twice returns `204`.
- **Not allowed:** cancelling a confirmed admission (fee already paid) or confirming/cancelling an expired hold → `409`. Acting on another student's hold → `403`.
- **Browser Back button** is not handled; pages switch inside the app without URL routing.

## Bonus: multiple backend processes
An in-memory lock only works within one process, so the lock is placed in PostgreSQL, which all backends share. Applying for a seat runs in a transaction that first locks the class row (`SELECT ... FOR UPDATE`), then counts active holds and inserts only if a seat is free. Requests for the same class therefore run one at a time, whichever backend receives them. Confirm and cancel can never increase held + confirmed, so they need no lock; each is a single atomic conditional `UPDATE`.
