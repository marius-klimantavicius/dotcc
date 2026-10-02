//type:fp
//options_all:--c11
//remark:[4.9] C11: Anonymous unions and anonymous structs
// 1/21/14  [EDGcpfe/14745]
//
// C11: Anonymous unions and anonymous structs
//
// In C11 mode, the front end now accepts anonymous unions and anonymous structs
// as specified in the C11 standard.
//
// Also, the configuration macro ALLOW_NONSTANDARD_ANONYMOUS_UNIONS has been
// removed.  The effect is as if the macro had been defined to be TRUE;
// configurations that set it to FALSE now elicit a compilation error (to avoid
// a silent change).
struct S {
  union {
    struct { int i, j; };
    struct { float x, y; };
  };
} s;
int printf(char const*, ...);
int main() {
  printf("%d\n", (char*)&s.j - (char*)&s.y);  // Prints "0" on common
  return 0;                                   // platforms.
}
