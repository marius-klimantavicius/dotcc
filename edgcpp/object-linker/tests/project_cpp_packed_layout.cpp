// Exact packed places: scalar, pointer, callback, floating and nested payloads.
static int add_three(int value) { return value + 3; }
static int marker = 19;
struct __attribute__((packed)) Packed {
  unsigned char tag;
  unsigned number;
  unsigned long long wide;
  int *pointer;
  int (*callback)(int);
  double real;
};
struct Inner { unsigned long long wide; unsigned values[2]; };
struct __attribute__((packed)) Nested { unsigned char tag; Inner inner; };
// Opaque pointer inputs carry no observed storage-origin alignment. A typed
// cast uses the caller's ordinary C/C++ alignment contract.
static unsigned *ordinary_void_member(void *value) { return &((Inner *)value)->values[0]; }
static unsigned *ordinary_char_member(char *value) { return &((Inner *)value)->values[1]; }
// Emission-only alignment probe: do not execute this unaligned C++ cast.
// Its explicit byte offset must not regain alignment through the final cast.
unsigned raw_offset_alignment_probe(unsigned *source) {
  return *(unsigned *)((char *)source + 1);
}
struct __attribute__((packed)) PackedStart { unsigned value; };
union AlignedSource { unsigned long long alignment; PackedStart item; };
unsigned conditional_alignment_probe(PackedStart *opaque, AlignedSource *known, bool choose) {
  return *(unsigned *)&(choose ? opaque : &known->item)->value;
}
static unsigned *ordinary_conditional_member(void *opaque, Inner *known, bool choose) {
  return &((Inner *)(choose ? opaque : known))->values[0];
}

#pragma pack(push, 2)
struct Pragma { unsigned char tag; unsigned long long wide; };
#pragma pack(pop)
struct __attribute__((packed, aligned(2))) AlignedPacked { unsigned char tag; unsigned number; };
struct ExplicitField { unsigned char tag; unsigned number __attribute__((aligned(8))); };
// These requests are ignored by the GNU layout rules. The source metadata
// must retain requested1 while the accepted physical alignment remains4.
struct ReducedField { unsigned number __attribute__((aligned(1))); };
struct __attribute__((aligned(1))) ReducedClass { unsigned number; };
struct __attribute__((packed)) Bits { unsigned long long low : 1; unsigned long long high : 63; };
static Packed table[2] = {
  { 7, 0x12345678U, 0x123456789abcdef0ULL, &marker, add_three, 1.25 },
  { 9, 0x87654321U, 0xfedcba9876543210ULL, &marker, add_three, -2.5 },
};
static Nested nested = { 2, { 0x1020304050607080ULL, { 31, 47 } } };
static AlignedPacked aligned_packed = { 5, 103 };
static ExplicitField explicit_field = { 6, 107 };
static ReducedField reduced_field = { 109 };
static ReducedClass reduced_class = { 113 };
static Packed copied(Packed value) { value.number += 5; return value; }
int main() {
  if (sizeof(Packed) != 37 || alignof(Packed) != 1 || sizeof(Pragma) != 10 || alignof(Pragma) != 2) return 1;
  if (alignof(AlignedPacked) != 2 || alignof(ExplicitField) != 8) return 2;
  if (aligned_packed.number != 103 || explicit_field.number != 107) return 3;
  for (unsigned offset = 1; offset != 16; ++offset) {
    unsigned char bytes[80] = {};
    Packed *place = (Packed *)(bytes + offset);
    bytes[offset - 1] = 0x5a;
    bytes[offset + sizeof(Packed)] = 0xa5;
    *place = table[offset & 1];
    if (place->pointer != &marker || *place->pointer != 19 || place->callback(4) != 7) return 4;
    if (place->real != ((offset & 1) ? -2.5 : 1.25)) return 5;
    place->number += offset;
    place->wide ^= 0x8877665544332211ULL;
    Packed value = copied(*place);
    if (value.number != table[offset & 1].number + offset + 5) return 6;
    if (value.wide != (table[offset & 1].wide ^ 0x8877665544332211ULL)) return 7;
    if (bytes[offset - 1] != 0x5a || bytes[offset + sizeof(Packed)] != 0xa5) return 8;
    Nested *other = (Nested *)(bytes + offset);
    *other = nested;
    other->inner.values[1] += offset;
    other->inner.wide += offset;
    if (other->inner.values[0] != 31 || other->inner.values[1] != 47 + offset ||
        other->inner.wide != 0x1020304050607080ULL + offset) return 9;
    // Array decay loses the containing packed layout. The generated raw
    // scalar access must still evaluate its address and RHS exactly once.
    unsigned index = 0;
    unsigned previous = other->inner.values[index++]++;
    if (index != 1 || previous != 31 || other->inner.values[0] != 32) return 13;
    unsigned updated = ++other->inner.values[index++];
    if (index != 2 || updated != 48 + offset || other->inner.values[1] != updated) return 14;
    unsigned assigned = (other->inner.values[--index] = 73 + offset);
    if (index != 1 || assigned != 73 + offset || other->inner.values[1] != assigned) return 15;
    unsigned multiplied = (other->inner.values[index--] *= 3);
    if (index != 0 || multiplied != 219 + offset * 3 || other->inner.values[1] != multiplied) return 16;
    // Casting a known packed address through void*/char* must not discard
    // its actual weaker alignment before the raw scalar load/store.
    unsigned roundtrip = ++*(unsigned *)(void *)&other->inner.values[0];
    unsigned second = (*(unsigned *)(char *)&other->inner.values[1] += 2);
    if (roundtrip != 33 || second != multiplied + 2) return 17;
  }
  Inner ordinary = { 101, { 103, 107 } };
  if (ordinary_void_member(&ordinary) != &ordinary.values[0] ||
      ordinary_char_member((char *)&ordinary) != &ordinary.values[1]) return 18;
  *ordinary_void_member(&ordinary) += 7;
  *ordinary_char_member((char *)&ordinary) += 11;
  if (ordinary.values[0] != 110 || ordinary.values[1] != 118) return 19;
  if (ordinary_conditional_member(&ordinary, &ordinary, true) != &ordinary.values[0] ||
      ordinary_conditional_member(&ordinary, &ordinary, false) != &ordinary.values[0]) return 20;
  Pragma pragma = { 1, 0x1020304050607080ULL };
  pragma.wide += 7;
  if (pragma.wide != 0x1020304050607087ULL) return 10;
  Bits bits = {};
  bits.low = 1;
  bits.high = 0x123456789abcdefULL;
  if (bits.low != 1 || bits.high != 0x123456789abcdefULL) return 11;
  if (alignof(ReducedField) != 4 || alignof(ReducedClass) != 4 ||
      reduced_field.number != 109 || reduced_class.number != 113) return 12;
  return 0;
}
