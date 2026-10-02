//options_all:-r -x -tused
//options: --strict;rp

extern "C" int printf(char *, ...);
enum E { a = -1, b = 7 };
struct S {
  E e : 3;
} x = { a }, y = { b };
int main() {
  printf("%d\n", x.e == a);
  printf("%d\n", y.e == b);
}

