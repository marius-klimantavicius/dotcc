# Blink host adapters

This directory contains the authored C# bridges and C ABI adapters that connect
the translated upstream library to the managed host implementation.

- `*.cs`: bridge implementations, linked directly into `TranslatedBlink` from
  this directory; edit these originals through `ManagedConsumer.slnx`.
- `*.c`: required C ABI adapters included in the selected translation closure.
- `include/`: adapter headers. Native builds using the C files directly must add
  this directory to their include search path; campaign runners snapshot the
  selected headers alongside their frozen inputs.
- `scripts/`: reviewed upstream staging helpers and host-call inventory tooling.
- `docs/`: retained detailed adapter documentation.

The implementation project remains at `../Managed.Emulation.Host/` and the
owning C# execution API at `../Managed.Emulation.Execution/`.
`../../config/host-bindings.json` is the explicit binding/source manifest.
Generated product projects reference authored C# sources and projects directly;
private qualification snapshots are separate from the active product.
