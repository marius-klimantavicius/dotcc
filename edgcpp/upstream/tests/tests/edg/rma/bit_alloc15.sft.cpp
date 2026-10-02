//options_all:-r -x -tused
//options: --cfront_3.0;rp

struct A {
  char a;
  int b;
};
 
struct B {
 char a;
 int b:1;
};
 
struct B2 {
  char a;
  char b:1;
};

struct C {
  char a;
  short b:1;
};
 
struct D {
  char a;
  long b:1;
};

extern "C" int printf(char *, ...);
main() {
 /* Expected values are those produced by SunCC3 (cfront) -- set
    TARG_BIT_FIELD_CONTAINER_SIZE to -1. */
 printf("sizeof(A) = %d\t(expecting 8)\n", (int)sizeof(A));
 printf("sizeof(B) = %d\t(expecting 4)\n", (int)sizeof(B));
 printf("sizeof(B2) = %d\t(expecting 2)\n", (int)sizeof(B2));
 printf("sizeof(C) = %d\t(expecting 2)\n", (int)sizeof(C));
 printf("sizeof(D) = %d\t(expecting 4)\n", (int)sizeof(D));
}

