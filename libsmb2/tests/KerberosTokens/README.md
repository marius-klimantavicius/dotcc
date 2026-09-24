# Kerberos token regression checks

Run without a KDC, network share or Windows logon credentials:

```sh
dotnet run --project libsmb2/tests/KerberosTokens -c Release
dotnet run --project libsmb2/tests/KerberosTokens -c Release \
  -p:Libsmb2GeneratedProject="$PWD/libsmb2/generated/TranslatedLibsmb2.Raw/TranslatedLibsmb2.csproj"
```

The executable uses the actual managed provider callbacks and locally generated
AES keys/AP-REPs. Every server-response fixture first calls `SendInitialToken()`
to return a prepared AP-REQ without completing authentication. It then calls
`CompleteWithServerToken(...)`, including `null` to model successful SMB session
setup with no security buffer. That empty completion clears the outgoing AP-REQ
and exports the service-ticket key only for a non-mutual context. Mutual contexts
still require a validated AP-REP; failed contexts cannot recover via empty input.

It also covers non-mutual request options and absence of an AP-REQ subkey,
DER and BER SPNEGO completion without AP-REP, wrapped and unwrapped AP-REP,
AP-REP subkey export, explicit Kerberos mechanism selection, failed/incomplete
negotiation, sticky failure and mandatory signed final session setup even when
SMB encryption disables the ordinary signing flag. SSPI package/mutual-authentication
policy is checked locally; no SSPI native calls are made on Linux.

The test's signed-header prerequisite checks are not an end-to-end SMB signature
verification test. Actual signature verification remains in translated
`session_setup_cb`, after key export and derivation and before TREE_CONNECT.

Remote qualification still requires password, keytab and FILE-cache connections
on Windows/Linux, plus Windows current-logon SSPI. Exercise signed SMB2 and
encrypted SMB3 list/read/write operations and confirm Kerberos was selected.
Neither this executable nor a successful build establishes those results.

The earlier 75-check fixture skipped initial token emission and did not cover
empty final security buffers. Passing offline checks does not establish live
keytab/password/FILE-cache interoperability or Windows SSPI interoperability.
