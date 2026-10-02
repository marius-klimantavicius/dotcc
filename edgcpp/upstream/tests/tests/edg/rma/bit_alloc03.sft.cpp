//options_all:-r -x -tused
//options: --strict;cn:;rp

extern "C" int printf(char*, ...);
struct foo {
        int a;
        unsigned char :0;
        char b;
        unsigned char :0;
        int x : 12, y : 4, : 0, : 4, z : 3;
        char c;
} ;

main() {
  union { foo a; unsigned int i[5]; };
  i[0] = i[1] = i[2] = i[3] = i[4] = 0;
  a.a = a.b = a.c = a.x = a.y = a.z = 1;
  printf("%08x %08x %08x %08x %08x\n", i[0], i[1], i[2], i[3], i[4]);
}

