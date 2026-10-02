//options_all:-r -x -tused
//options: --strict;rp

enum E { m,n };
struct A { char x:9;
           E y: 16;
           int z: 1000;
         } a;
extern "C" int printf(char *, ...);
int main() {
  printf("%d\n", sizeof(a));
}

