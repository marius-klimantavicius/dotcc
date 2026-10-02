# EDG GNU Pretty Printers

This Python library provides significantly improved GDB pretty printers to
the EDG front end for a large number of front end types.

## Setup

> [!NOTE]
>
> Setup is automated for gdb use within the EDG Docker development containers.

EDG Python libraries "EDG pylibs" must be exposed in your `PYTHONPATH`
environment variable.  See the main project's
[development guide](../../../HACKING.md) for help with this.

Then, you'll need to modify your `.gdbinit` file (creating it if it doesn't
exist); add the following to the file:

```
python

from edggpp import register_printers
register_printers()

end
```

To verify the printers have been registered successfully, start gdb and run
`info pretty-printer`. You should see output like the following:

```
global pretty-printers:
  ...
  pretty-cpfe
    ...
...
```

## Licensing

The EDG GNU Pretty Printers are licensed under GPL to ensure they're legally
compatible with GDB.
