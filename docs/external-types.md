# Authored target types

`--type-name NAME` registers an additional C type name, using the same parser
seed mechanism as runtime types such as `div_t`. Repeat the option for multiple
names. Dotcc preserves the name in generated C# without generating a struct or
typedef for it. Supply that C# type in an additional source file in the generated
project (or in a referenced assembly accessible by that name).

For example, the following source needs no C declaration of `ManagedHandle`:

```c
ManagedHandle identity(ManagedHandle value) { return value; }
```

```sh
dotcc example.c --emit=managedlib --type-name ManagedHandle -o generated
```

Names and optional unmanaged storage contracts can also be supplied in the
version-1 `--overrides-file` profile:

```json
{
  "version": 1,
  "externalTypes": [
    { "name": "ManagedHandle", "layout": { "size": 4, "alignment": 4 } },
    { "name": "OtherHandle" }
  ]
}
```

Layout is optional. Name-only registration supports pointers and pass-through
signatures. Supply layout when dotcc must calculate `sizeof`, `_Alignof`,
aggregate field offsets or array storage. Layout requires a positive size and a
power-of-two alignment no greater than 128; size must be divisible by alignment.
The authored C# type must actually have this unmanaged layout. Dotcc does not
reflect over the authored assembly or insert runtime layout assertions.

The layout describes storage only. It does not introduce member names, field
types, conversions or arithmetic operators. Accessing members of an external
type is diagnosed; use a C header definition if the compiler needs to know its
members. C definitions cannot redefine registered external names. Registration
does not replace bundled runtime types or reserved identifiers.

CLI names and profile entries are merged; repeated names deduplicate, and a
name-only CLI entry preserves a profile's layout. Conflicting explicit layouts
fail. The immutable profile hash includes the normalized names and layouts.
Each separately compiled object records its external type contracts; linking
conflicting layouts or generated definitions of an external type fails. Use the
same type registration when compiling every translation unit sharing its ABI.

These settings are compiler configuration only. They do not inject C source,
alter macros, generate host behavior, or enable target-language operator binding
beyond the compiler's existing expression lowering.
