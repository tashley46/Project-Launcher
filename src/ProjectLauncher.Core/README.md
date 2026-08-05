# ProjectLauncher.Core

This project is the application/API boundary. Validation, commands, queries, and use-case orchestration live here.

Commands and queries are grouped first by the domain entity they affect, then by operation type. Infrastructure implementations are injected behind interfaces so domain entities remain persistence- and UI-independent.

