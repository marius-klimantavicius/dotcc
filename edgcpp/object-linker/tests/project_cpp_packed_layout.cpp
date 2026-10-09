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
  }
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
