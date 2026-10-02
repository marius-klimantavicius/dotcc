// Synthetic shapes used by libc++'s string representation: EDG gives the
// anonymous aggregate a compiler-generated parent field with ordinary storage.
struct Packed {
  struct __attribute__((packed)) { unsigned long flag : 1; unsigned long count : 63; };
  int tail;
};
struct Nested {
  union { Packed value; unsigned char raw[sizeof(Packed)]; };
};
int main() {
  Nested first{};
  first.value.flag = 1;
  first.value.count = 0x123456789UL;
  first.value.tail = 19;
  Nested second = first;
  second.value.count += 7;
  if (first.value.count != 0x123456789UL || second.value.count != 0x123456790UL) return 1;
  if (second.value.flag != 1 || second.value.tail != 19 || (second.raw[0] & 1) != 1) return 2;
  second.value.flag = 0;
  return first.value.flag == 1 && (second.raw[0] & 1) == 0 ? 0 : 3;
}
