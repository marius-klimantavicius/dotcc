//options_all:-r -x -tused
//options: --strict;rp

extern "C" int printf(char*, ...);
struct A {
  char c1[1];
  unsigned int bf1:12;
  char c2[2];
  unsigned int bf2:12;
  char c3[3];
  unsigned int bf3:12;
  char c4[4];
  unsigned int bf4:12;
  char c5[5];
  unsigned int bf5:12;
  char c6[6];
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

