# AGENTS.md

## Project overview

This repository contains the .NET Standard LDAP client library in `src/Novell.Directory.Ldap.NETStandard` and its test projects under `test/`.

- Library project: `src/Novell.Directory.Ldap.NETStandard/Novell.Directory.Ldap.NETStandard.csproj`
- Unit tests: `test/Novell.Directory.Ldap.NETStandard.UnitTests/Novell.Directory.Ldap.NETStandard.UnitTests.csproj`
- Functional tests: `test/Novell.Directory.Ldap.NETStandard.FunctionalTests/`
- Stress tests: `test/Novell.Directory.Ldap.NETStandard.StressTests/`

The library targets multiple frameworks, including `netstandard2.0`, `netstandard2.1`, `net6.0`, `net8.0`, and `net10.0`.

## Working conventions

- Keep changes focused and minimal; this codebase is a compatibility-oriented library, not a large app framework.
- Prefer matching existing patterns and naming conventions already used in the source tree.
- Do not broaden scope beyond the requested bug, API, or feature work.
- Prefer tests that validate real behavior over mocks when exercising library logic.
- Make the smallest fix that addresses the root cause.

## Build and validation

Use the project-level commands below for verification.

### Restore + build

```bash
dotnet build Novell.Directory.Ldap.sln -v minimal
```

### Unit tests

```bash
dotnet test test/Novell.Directory.Ldap.NETStandard.UnitTests/Novell.Directory.Ldap.NETStandard.UnitTests.csproj -v minimal
```

If only a single target framework is needed, prefer a framework-specific run such as:

```bash
dotnet test test/Novell.Directory.Ldap.NETStandard.UnitTests/Novell.Directory.Ldap.NETStandard.UnitTests.csproj -f net10.0 --no-restore -v minimal
```

## Environment notes

- The repo is CI-oriented and expects .NET SDK/runtime availability for the target frameworks being validated.
- In local environments with only newer runtimes installed, a multi-target test run may fail for older runtime test hosts; prefer the target framework that is available locally when validating a focused change.
- The repo has a functional/stress test setup that depends on LDAP server infrastructure; those tests are not a replacement for small, focused unit validation.

## Useful references

- README: [README.md](README.md)
- Changelog: [ChangeLog.md](ChangeLog.md)
- Solution file: [Novell.Directory.Ldap.sln](Novell.Directory.Ldap.sln)

## When generating patches

- Prefer existing code style and naming used in the surrounding file.
- Add or update the most relevant unit test for the behavior being changed.
- Avoid unrelated cleanup or platform churn in the same patch.
- If a fix is only for a specific runtime or scenario, document that clearly in the final summary.
