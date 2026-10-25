# Guest mapping hints

`python3 -B blink/tests/GuestMappingHints/run.py` runs an ordinary Linux mmap
fixture and the actual authored memory adaptation against a private native
Blink/HostMemory build. Receipts and logs are retained under
`artifacts/guest-mapping-hints/` on success and failure. No shared product
translation or generated C# edit is performed.

The fixture preserves an existing page while requesting a nonfixed, unaligned
hint whose rounded range crosses it. The metadata check exercises empty
page-table entries immediately before occupied neighbors at 512 GiB, 1 GiB and
2 MiB boundaries. It verifies free-range detection, selection of a genuinely
free range, exclusive range ends, original bytes, and final owner cleanup.
It never accesses an invalid guest address or injects a fault.

The emitted `fixture-static` ELF can be imported and executed through a fresh
public product by the coordinator. Its expected stdout is
`mapping hints passed\n` and exit code is zero. Native checks alone do not
qualify translated execution or CoreCLR startup.
