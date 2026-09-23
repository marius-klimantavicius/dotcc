# IMDSv2 through the public machine API

Build the real AWS SDK guest as a static Linux x64 musl NativeAOT executable:

```sh
docker build --platform linux/amd64 \
  --output type=local,dest=blink/build/imds-nativeaot blink/tests/ImdsGuest
bash blink/scripts/translate.sh --offline
python3 blink/scripts/test-imds.py
```

The guest pins `AWSSDK.Core` 4.0.102.6. It uses the ordinary
`169.254.169.254` endpoint without an SDK endpoint override. It obtains an IMDSv2
token, reuses it across HTTP connections and reads the instance identity.
`InstanceProfileAWSCredentials` discovers the configured role, obtains credentials,
reads its cache, then clears that cache and retrieves credentials again. IMDSv1
fallback is disabled. All credentials are explicit fake test values; no AWS
account or access to real metadata is needed.

The runner qualifies JIT and NativeAOT consumers in both `InProcess` and
`SeparateProcess` modes. Each cell starts two machines with different identities
and credentials, then executes the guest again in the first machine. It checks
exact guest output, normal exit, released resources, translated instruction
counts and the actual worker binary. Focused host checks run under JIT and AOT
through guest socket APIs, including token expiration/isolation, fragmented valid
requests, HEAD, UTF-8 user data, concurrent requests and ordinary disposal.
The same guest also runs natively twelve times against this simulator's private
loopback endpoint as a control; only that native control sets an endpoint override.
The host checks exercise concurrent page registration and lookup during growth.

Receipts, commands, hashes and results remain in `blink/artifacts/imds/attempt-*`.
The native guest is not run against the host's real link-local metadata address.
`--jit-only` is a development check, not full qualification.

For a quick individual run after building the guest:

```sh
dotnet run --project blink/tests/ImdsMachine -c Release -- \
  blink/build/imds-nativeaot/ImdsGuest InProcess blink/artifacts/imds-manual
```

Replace `InProcess` with `SeparateProcess` to try the worker. See
[IMDSV2.md](../../src/Managed.Emulation/IMDSV2.md) for application configuration.
