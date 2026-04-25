# Contributing to MCP OS Doctor

Thanks for your interest in contributing! Here are the guidelines to keep things consistent and smooth.

## General

- **Follow project rules**: respect existing code style, naming conventions, and architecture.
- **Be nice**: constructive feedback, patience, and respect go a long way.

## Code Quality

- **Add tests for any new code**: every new feature, tool, or provider should include unit tests. Check the `src/tests` folder for examples.
- **Warnings are errors**: the project uses `TreatWarningsAsErrors`, so make sure your code compiles cleanly.
- **Analyzers are enabled**: Roslynator runs on every build. Address any diagnostics it raises.

## Architecture

- **Read-only by design**: this server must never modify the system state, execute commands, or open network connections. If your contribution introduces any of these, it will not be accepted.
- **Platform abstraction**: platform-specific code belongs in a `McpOsDoctor.Platform.*` project (e.g., `McpOsDoctor.Platform.Windows`). Core interfaces and shared logic live in `McpOsDoctor.Core`, and tool definitions in `McpOsDoctor.Tools`.
- **New platform support**: if adding a new platform (e.g., Linux, macOS), create a corresponding `McpOsDoctor.Platform.<Name>` project and update the CI/CD pipeline to include publishing and packaging for it.

## Submitting Changes

- Fork the repository and create a feature branch from `master`.
- Keep pull requests focused: one feature or fix per PR.
- Write a clear PR description explaining **what** changed and **why**.

## License

By contributing, you agree that your contributions will be licensed under the [GPLv3](LICENSE.md).
