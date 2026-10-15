The snapshot and RBU lifecycle slice passes all five measured native/JIT/NativeAOT
phases. See [harness and preconditions](../tests/SqliteMaintenance/README.md) and
[frozen receipt](../acceptance/sqlite/maintenance-report.json).

Two captured 48/1 snapshot handles retain the same WAL generation across writer
commits. The old reader remains unchanged; snapshot comparison signs are -1/1/0,
old/new snapshots replay their respective rows, recover runs outside a read
transaction, both handles free before WAL history is released, and final
checkpoint/integrity pass.

The captured 416/8 RBU handle processes genuine update/delete/insert input and a
secondary index. One actual step is saved and closed. Reopen resumes progress 1,
then advances through actual stages 1/2/3/4 to DONE; repeated step and close retain
DONE. Final rows, index order and integrity match native exactly. No step count
was predicted before the native observation.

All tracked owners release, no cleanup error is recorded, and generated process
maps exclude native SQLite. NativeAOT explicitly pins clang 18; publish passes in
73.16 seconds with sampled process-tree peak 3.227 GB. Short execution samples
can miss startup peaks; kernel waited RSS remains separately recorded.

Invalidated/cross-generation snapshot behavior, RBU VACUUM/custom VFS/failure
variants and wider concurrency/failure/provider contracts remain outside this
finite proof. T4 still needs its separate concurrency, tokenizer, failure,
readability and restart acceptance rows.
