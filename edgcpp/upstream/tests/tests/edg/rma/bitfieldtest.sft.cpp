//options_all:-r -x -tused
//options: --strict;rp

extern "C" int printf(char*, ...);
struct A {
  int i:2;
  signed int si:2;
  unsigned int ui:2;
  short j:2;
  signed short sj:2;
  unsigned short uj:2;
  char k:2;
  signed char sk:2;
  unsigned char uk:2;
  long l:2;
  signed long sl:2;
  unsigned long ul:2;
};
int main() {
  A a;
  a.i = -1;
  a.si = -1;
  a.ui = -1;
  a.j = -1;
  a.sj = -1;
  a.uj = -1;
  a.k = -1;
  a.sk = -1;
  a.uk = -1;
  a.l = -1;
  a.sl = -1;
  a.ul = -1;
  printf("%d %d %d\n", (int)a.i, (int)a.si, (int)a.ui);
  printf("%d %d %d\n", (int)a.j, (int)a.sj, (int)a.uj);
  printf("%d %d %d\n", (int)a.k, (int)a.sk, (int)a.uk);
  printf("%d %d %d\n", (int)a.l, (int)a.sl, (int)a.ul);
}

