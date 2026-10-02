//type:rp
//options:--ms_c++20 --microsoft_version 1936 --target linux_aarch64
//options_all:-w

struct B
{
  alignas(16) char arr[2];
};

struct D : B
{
  char d;
};

extern "C" int printf(const char *, ...);
extern "C" void *memcpy(void *, const void *, decltype(sizeof 0));

int main()
{
  int i[] = { 1, 2, 3, 4 };
  D d;
  int j[] = { 5, 6, 7, 8 };

  printf("%ld %ld %ld %ld\n",
      sizeof(B), sizeof(D),
      __builtin_offsetof(D, arr), __builtin_offsetof(D, d));

  // Make sure that non-static data member access uses the same layout
  memcpy(&d, "012345678XXXXXXX", 16);
  printf("%c %c %c\n", d.arr[0], d.arr[1], d.d);
}
