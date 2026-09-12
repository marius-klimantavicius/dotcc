# Selected decoder corpus

`corpus.json` attributes 75 cases to the pinned upstream `FrameTest.cpp` and
`TransportParamTest.cpp` by source path, line and SHA256. Valid RESET_STREAM,
STOP_SENDING, CRYPTO and MAX_DATA values and malformed varint mask combinations
come from those tests. Proper-prefix mutations are labeled as derived cases.
CIBIR bounds, GREASE and reliable-reset flag lengths, and a 21-byte version-info
blob exercise the transport-parameter decoder. The version bytes are explicitly
initialized here; the upstream test only specifies their length.

The extra version-info cases decode twice, reject duplicate parameters after
allocation, reject a malformed following ID, and replace previously allocated
state with invalid input. Every case verifies allocation drain. Decoder support
for an extension does not select it for the product's negotiated API profile.

The native control compiles **unchanged** `frame.c` and `crypto_tls.c` with their
ordinary upstream Linux headers. Function sections and linker garbage collection
remove unexercised functions. Only actual tracked `malloc`/`free` services are
provided; no parser, transport, TLS or allocation-success stub replaces original
logic. Compiler dependencies, including system headers, are recorded and hashed.
The managed control calls the already translated functions with the full real
BCL host installed, without starting the transport library or workers.

Each record contains ID, Boolean decoder result, frame offset, three decoded
scalars, copied payload bytes, live allocations before cleanup, and allocations
after cleanup. Failed frame output fields are unspecified and normalized to zero;
the real failed offset is still compared. Transport-parameter decode exposes no
offset, so its field is `NA`. Its scalar columns are flags, CIBIR length and offset;
the payload column is the owned version-info blob. The replacement case prints
the second, failing decode after checking the first succeeded and allocated.

Run `python3 msquic/scripts/test-malformed-corpus.py` under the repository's
serialized test slot. `--native-only`, `--jit-only` and `--variants` are diagnostic
subsets; only the complete native/raw/optimized/JIT/NativeAOT run can set
`artifacts/malformed-corpus/results.json:passed=true`. Exact output is compared
against the native executable, including successful payload offsets and every
allocation count. These are selected decoder controls, not all upstream test
declarations, an endpoint malformed-packet test, or a fuzzing claim.
