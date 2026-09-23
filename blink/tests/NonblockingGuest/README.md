# NativeAOT asynchronous HTTP guest

Build the static Linux x64 musl guest using the pinned .NET AOT SDK image:

```sh
docker build --platform linux/amd64 \
  --output type=local,dest=blink/build/httpclient-nativeaot \
  blink/tests/HttpClientGuest
```

After generating the Blink delivery with `bash blink/scripts/translate.sh`, run:

```sh
python3 blink/scripts/test-nonblocking-guest.py
```

The runner builds the public-machine consumer and its worker, executes a native
oracle, then runs JIT and NativeAOT consumers in both `InProcess` and
`SeparateProcess` modes. Each cell makes **18 HTTP requests**: two simultaneous
guests followed by a second execution in the first machine. Every guest makes
two sequential and four parallel `HttpClient.GetStringAsync` calls, requesting
connection closure to exercise fresh TCP connections. The managed BCL HTTP
responder echoes each request path; exact responses, captured output, normal
exit and released machine resources are required. A service-side rendezvous
on the first two requests establishes simultaneous guest execution without
timing assumptions.

The guest ELF is mounted read-only at `/work`. Each machine receives only an
explicit numeric IPv4 outbound grant to the responder's loopback port. This
fixture covers plain HTTP; it does not qualify DNS, IPv6, TLS or IMDS routing.
Loopback connect may complete immediately; focused host checks record naturally
observed pending connects without artificially delaying the transport.

Each attempt retains commands, logs, source/delivery/compiler identities,
runtime and worker executable evidence, and per-cell `result.json` files under
`blink/artifacts/nonblocking-guest/attempt-*`. Failed attempts are retained.
`--elf` selects another build of this guest. `--jit-only` and `--skip-build` are
development options; full fresh qualification uses the command above.
