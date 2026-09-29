# Direct API fixtures

This executable adds direct netconn and plain-altcp coverage beyond the owning
consumer's BSD-socket/raw paths. Each applicable IPv4/IPv6 case transfers a
256 KiB deterministic echo stream between generated owners over a bounded
managed Ethernet queue, verifies a duplicate-bind error, and closes/deletes
its endpoints. Netconn cases inspect chained netbufs and the `ERR_CLSD` EOF
contract. Altcp cases use explicit-owner receive/accept/connect callbacks and
remove registrations before freeing callback state.

Run with an ordinary project reference and the generated profile selection:

```bash
dotnet run --project lwip/tests/SocketApi -c Release -p:LwipProject=/absolute/path/TranslatedLwip.csproj
```

For a main-loop product pass `-p:LwipProfile=nosys`; netconn cases are inapplicable
there, and plain-altcp uses the serialized host/core entry path. The fixture's
transport thread pumps main-loop timeouts through that same serialized facade.

These cases do not by themselves complete the plan's whole `socket-api` suite:
select/poll/ioctl/options, concurrent close/cancellation and additional error
contracts remain separate required cases. Qualification status belongs in the
campaign receipts, not successful compilation of this test project.
