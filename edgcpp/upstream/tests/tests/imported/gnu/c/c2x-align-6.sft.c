//type: rp
//options: --c23
# 0 "./c2x-align-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c2x-align-6.c"







# 1 "./c11-align-6.c" 1





extern void abort (void);
extern void exit (int);
# 18 "./c11-align-6.c"
int
main (void)
{
  do { struct { char c; _Bool v; } x; if (alignof (_Bool) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; char v; } x; if (alignof (char) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; signed char v; } x; if (alignof (signed char) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; unsigned char v; } x; if (alignof (unsigned char) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; signed short v; } x; if (alignof (signed short) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; unsigned short v; } x; if (alignof (unsigned short) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; signed int v; } x; if (alignof (signed int) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; unsigned int v; } x; if (alignof (unsigned int) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; signed long v; } x; if (alignof (signed long) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; unsigned long v; } x; if (alignof (unsigned long) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; signed long long v; } x; if (alignof (signed long long) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; unsigned long long v; } x; if (alignof (unsigned long long) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; float v; } x; if (alignof (float) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; double v; } x; if (alignof (double) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; long double v; } x; if (alignof (long double) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; _Complex float v; } x; if (alignof (_Complex float) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; _Complex double v; } x; if (alignof (_Complex double) > __alignof__ (x.v)) abort (); } while (0);
  do { struct { char c; _Complex long double v; } x; if (alignof (_Complex long double) > __alignof__ (x.v)) abort (); } while (0);
  exit (0);
}
# 9 "./c2x-align-6.c" 2
