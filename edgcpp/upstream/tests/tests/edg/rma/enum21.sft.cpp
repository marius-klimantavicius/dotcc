//options_all:-r -x -tused
//options: --strict;cn:;rp

enum E { a = -1, b = 0, c = 1 };
struct S {
  E x : 4;
  E y : 2;
  E z : 1;
};
union SI { S s; int i; };
SI sa, sb, sc;

extern "C" int printf(char *, ...);
main() {
  sa.s.x = a; sa.s.y = a; sa.s.z = a;
  sb.s.x = b; sb.s.y = b; sb.s.z = b;
  sc.s.x = c; sc.s.y = c; sc.s.z = c;
  printf("sa: %x = %x, %x, %x\n", sa.i, sa.s.x, sa.s.y, sa.s.z);
  printf("sb: %x = %x, %x, %x\n", sb.i, sb.s.x, sb.s.y, sb.s.z);
  printf("sc: %x = %x, %x, %x\n", sc.i, sc.s.x, sc.s.y, sc.s.z);

  printf("sa.s.x %s a\n", sa.s.x == a ? "==" : "!=");
  printf("sa.s.y %s a\n", sa.s.y == a ? "==" : "!=");
  printf("sa.s.z %s a\n", sa.s.z == a ? "==" : "!=");

  printf("sc.s.x %s c\n", sc.s.x == c ? "==" : "!=");
  printf("sc.s.y %s c\n", sc.s.y == c ? "==" : "!=");
  printf("sc.s.z %s c\n", sc.s.z == c ? "==" : "!=");
}

