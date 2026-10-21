# Oracle adapter status controls — 2026-10-21

Supervisor executed `python3 tools/test-oracle.py` from the manual-csharp root:
exit 0, four tests passed (`Ran 4 tests in 0.000s`, `OK`).

The controls establish acceptance only for native exit 0, language rejection
only for exit 2, and no pass for catastrophe 4, arbitrary statuses or signals.
This Linux profile follows `host_envir.c:exit_compilation` and
`host_envir.h:172–180` (`RC_NORMAL=0`, `RC_ERROR=2`, `RC_CATASTROPHE=4`).
Missing processes/timeouts raise errors rather than matching an expectation.
Actual language diagnostics still receive independent inspection; an exit code
alone is not proof of diagnostic parity or semantic graph facts. Unreviewed
catastrophic malformed-source outcomes are verification gaps, not silently
accepted negative tests.

Independent reviewer inspected the actual tool change, unittest controls and
upstream status mapping and found no issue. The ensuing native M4 contract
receipts retain explicit `expected_exit_code` and actual output.

The profile/control suite subsequently grew to seven passing tests, including
default `--instantiate used`, explicit `none`, and preprocessing command
construction. Its executed receipt fingerprints both tool files in
`oracle-adapter-profile-tests.json`.

An important profile probe initially expected selected false static_assert to
reject but native returned 0: EDG defaults external template instantiation to
`none` (`ext_intf.rst:1649+`). The failed expectation receipt is retained as
`native-m4-selected-false-assert-contract.json` and is not an acceptance pass.
Explicit `--instantiate used` rejects the selected body while accepting the
unused definition. Selected-invalid dependent return versus losing-template
controls likewise discriminate body instantiation. All M4 acceptance commands
must retain this explicit profile; old no-template M3 witnesses are unaffected.

`tools/test-oracle-integration.py` explicitly invokes the native oracle for all
four selected/unused/losing controls. It fails if the body-instantiation flag
regresses and retains source/compiler/profile/actual status receipts. This tool
requires native EDG only when explicitly selected; default managed tests do not.
