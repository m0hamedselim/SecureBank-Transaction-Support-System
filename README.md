# SecureBank: Transaction & Support System 🏦

SecureBank is a multi-threaded client-server banking application built using **C#.NET** and native **Socket Programming**. The system demonstrates complex network architectures by utilizing both connection-oriented (TCP) and connectionless (UDP) protocols seamlessly within a single ecosystem.

## 🌟 Key Features

* **Dual-Protocol Architecture:** * **TCP (Port 8000):** Used for data-critical tasks requiring 100% reliability (Authentication, Financial Operations, and Chatbot Support).
  * **UDP (Port 8001):** Used for fast, low-latency, real-time requests (Live Currency Exchange Ticker).
* **Multi-Threaded Server:** Spawns a dedicated thread per connected TCP client to prevent blocking and isolate client sessions.
* **Thread-Safe State Management:** Enforces rigorous data integrity for the global shared balance and logging systems using explicit thread synchronization (`lock`).
* **Smart AI Support Chatbot:** Features a responsive keyword-driven support chatbot (`HELP`, `HOURS`, `LOAN`).
* **Persistent Audit Logging:** Maintains a complete server-side text-based transaction log (`server_log.txt`) with precise timestamps and endpoint tracking.
* **Graphical Interface:** Built with a responsive Windows Forms GUI Application featuring asynchronous (`async/await`) network patterns.

---

## 🏗️ System Architecture

The project adheres strictly to the classic Client-Server design model:

```text
                       ┌────────────────────────────────────────┐
                       │           SecureBank Server            │
                       │  ┌──────────────────────────────────┐  │
                       │  │   Multi-Threaded TCP Listener    │  │
                       │  │          (Port 8000)             │  │
                       │  └────────────────┬─────────────────┘  │
                       │                   │                    │
                       │  ┌────────────────┴─────────────────┐  │
                       │  │    Background UDP Listener       │  │
                       │  │          (Port 8001)             │  │
                       │  └──────────────────────────────────┘  │
                       └───────────────────┬────────────────────┘
                                           │
                                           ▼
                        ┌──────────────────────────────────┐
                        │        WinForms GUI Client       │
                        │ 🛠️ TCP Banking & Support Chat    │
                        │ ⚡ UDP Live Forex Ticker         │
                        └──────────────────────────────────┘


