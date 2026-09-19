# Current test scope

The user requested that fault injection be ignored unless an explicit
corresponding test exists in the pinned upstream library. The coordinator and
both subagents were restarted to apply this rule. Their review was read-only.

These upstream cases are present under the pinned snapshot's `tests/` directory:

| Upstream test | Explicit behavior |
| --- | --- |
| `test_0102_ls_basic_socket_error.sh`, `test_0212_cp_valgrind_socket_error.sh`, `test_0302_cat_valgrind_socket_error.sh` | `ld_sockerr.c` injects a local readv error with EBADF; it does not reset a TCP peer. |
| `test_0103_ls_basic_valgrind_malloc_error.sh` | Fails an allocation ordinal through `ALLOC_FAIL`, with native process/leak checks. |
| `test_0310_cancel_pdu.sh`, `prog_cat_cancel.c` | Frees a newly created OPEN PDU, verifies its callback is not invoked, then continues normal operations. |
| `test_0311_open_timeout.sh`, `prog_open_timeout.c` | Tests unanswered CREATE operations for open/opendir using a special server; ordinary Samba is not that fixture. |

This inventory does not claim those upstream tests were executed. A related error
class is insufficient to treat a different custom mechanism as the same test.

The default campaign therefore excludes the custom TCP-reset peer and synthetic
failed-first-close/pending-read cases. Their earlier results are historical.
The unfinished additional cleanup investigation is preserved outside the product
under `build/deferred-cleanup-investigation/`; no source patch was integrated.

Normal file operations, invalid API inputs, wrong credentials, missing shares,
crypto known-answer checks, cancellation before submission, ordinary disposal,
concurrency, and GC/finalizer ownership checks remain functional validation.
The known upstream compound-metadata cleanup limitation remains documented;
the current tests do not establish full failure-path acceptance.
