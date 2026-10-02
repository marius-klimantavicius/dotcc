//options_all:-r -x -tused
//options: --strict;rp

extern "C" int printf(char*, ...);
struct A {
  char c;
  unsigned int bf1:1;
  short s;
  unsigned int bf2:1;
  int i;
  unsigned int bf3:1;
  long l;
  unsigned int bf4:1;
  double d;
  unsigned int bf5:1;
};
A a;
int main()
{
  union { A a; unsigned int i[10]; };
  for (int j = 0; j < 10; j++) i[j] = 0;
  a.bf1 = a.bf2 = a.bf3 = a.bf4 = a.bf5 = 1;
  printf("%08x %08x %08x %08x %08x\n", i[0], i[1], i[2], i[3], i[4]);
  printf("%08x %08x %08x %08x %08x\n", i[5], i[6], i[7], i[8], i[9]);
}

