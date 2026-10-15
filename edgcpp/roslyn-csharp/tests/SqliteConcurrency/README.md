# Finite concurrency barriers

This slice uses two real worker processes with JSONL stdin/stdout command IDs and
acknowledgments. The orchestrator sends the next command only after the current
one succeeds. There are no sleeps or elapsed-time success assertions. Watchdogs
only bound broken processes/barriers.

Process A creates a WAL database and holds an acknowledged write transaction.
Process B installs a busy handler returning zero and attempts a competing write;
step, primary/extended error, finalization and callback argument/count are copied
before later operations change error state. B starts a reader, A commits, and B
still reads its old snapshot. A passive checkpoint is checked for pending frames
while B pins that history. After B ends the transaction it reads new rows; A
truncates the WAL and the database is closed/reopened. All callback contexts
outlive registrations and are unregistered before close.

The shared-cache phase uses two explicit FULLMUTEX/SHAREDCACHE handles on a fresh
rollback-mode file, without changing global shared-cache configuration. A table
write lock makes the second handle's statement return LOCKED_SHAREDCACHE. The
test registers `unlock_notify` while the failed statement still retains its
blocker, releases the writer, records the actual callback, finalizes the failed
statement once, then prepares and succeeds on retry. The callback records only
its supplied native context array; it invokes no SQLite APIs.

The thread phase uses two real threads with separate FULLMUTEX/PRIVATECACHE
connections and three read rounds. A barrier after open and before/after each
round permits simultaneous reads while making their ownership and joins
explicit. Reports order results by thread ID and round, without asserting which
thread executed first. ctypes CDLL calls release the GIL; generated workers use
ordinary managed Threads and direct generated application calls.

Actual public retained APIs are ordinary fixed signatures. Busy callback is
Cdecl `int(void*, int)`; unlock callback is Cdecl `void(void**, int)`. Callback
exceptions are contained and reported. The selected public inventory and actual
published Generated.cs declarations establish all C# aliases and signatures.
This test does not infer source overloads, inspect a raw model database or load
native SQLite into the generated application.

The coordinator schedules native observations first:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteConcurrency/run-concurrency.py \
  --mode native --artifacts /absolute/new/native-concurrency
```

Only `native-worker.py` loads the absolute native oracle from the exact accepted
build receipt and hash, in separate processes. `run-concurrency.py` records the
complete command transcript, worker closure, callback errors, log hashes and
source freeze. Native observations, not manually predicted row constants, are
the comparison authority. The prepared native sources have not been executed by
their author; coordinator observations establish their actual behavior.

Deadlock cycles, multiple grouped waiters, cancellation races, custom VFS,
failure injection, interrupted processes, prolonged contention and broader
provider/thread contracts remain separate. FTS tokenizer/callback interfaces are
also separate from this source change.
