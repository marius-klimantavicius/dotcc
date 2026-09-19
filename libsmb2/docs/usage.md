# Build and run

The Linux x64 campaign requires .NET 10 SDK and Python 3.12+. Native controls also
require CMake, a C compiler, and Docker; NativeAOT requires the SDK's native
toolchain prerequisites. First source acquisition and tool restore use the network.

From the repository root:

```sh
./libsmb2/scripts/translate.sh
dotnet build libsmb2/ManagedConsumer.slnx -c Release
dotnet run --project libsmb2/samples/ManagedConsumer -c Release -- --help
```

Translation resolves inputs relative to the script and also works when invoked
by absolute path from another directory. It verifies the pinned source, emits all
53 units, links and builds raw C#, runs the semantic postprocessor, and builds
the product before promotion to `generated/TranslatedLibsmb2/`. Failed attempts
leave the prior product in place, with a failed receipt in
`artifacts/translation/result.json`. Check that receipt before using old output.
`--jobs N` controls independent unit emission (1–16, default 4).

To use an existing share, read the password without echoing it:

```sh
read -rs -p 'SMB password: ' LIBSMB2_PASSWORD
export LIBSMB2_PASSWORD
export LIBSMB2_DOMAIN=WORKGROUP
dotnet run --project libsmb2/samples/ManagedConsumer -c Release -- \
  localhost:445 probe username 0311 --encrypt
unset LIBSMB2_PASSWORD
```

The sample lists entries, exclusively creates a uniquely named file, writes and
reads back 65,537 exact bytes, checks EOF, and deletes its file. It needs a writable
share. Omit `--encrypt` for the signed profile; choose `0202`, `0210`, `0300`,
`0302`, or `0311` explicitly. [API behavior and cancellation](api.md) describe the
current facade's limits.

NativeAOT publication and isolated Samba checks:

```sh
dotnet publish libsmb2/samples/ManagedConsumer -c Release -r linux-x64 -p:PublishAot=true
./libsmb2/scripts/oracle.sh
./libsmb2/scripts/oracle.sh --managed jit
./libsmb2/scripts/oracle.sh --managed aot
./libsmb2/scripts/oracle.sh --managed jit --raw
./libsmb2/scripts/oracle.sh --managed aot --raw
```

The oracle creates a disposable Samba container and random credentials, binds a
random loopback port, captures logs, and removes the container and password file
on exit. Generated product checks are still being brought up; command availability
does not imply passing validation. See [current evidence](validation.md).
