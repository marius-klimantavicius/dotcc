//type: rp
//options: 
# 0 "./torture/vshuf-v2di.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/vshuf-v2di.c"







typedef unsigned long long V __attribute__((vector_size(16)));
typedef V VI;




# 1 "./torture/vshuf-2.inc" 1
# 15 "./torture/vshuf-v2di.c" 2
# 1 "./torture/vshuf-main.inc" 1


extern void abort (void);


V a, b, c, d;
# 28 "./torture/vshuf-main.inc"
__attribute__((noinline, noclone)) void test_0 (void) { VI mask = { 0, 0 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_1 (void) { VI mask = { 0, 1 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_2 (void) { VI mask = { 0, 2 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_3 (void) { VI mask = { 0, 3 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_4 (void) { VI mask = { 1, 0 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_5 (void) { VI mask = { 1, 1 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_6 (void) { VI mask = { 1, 2 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_7 (void) { VI mask = { 1, 3 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_8 (void) { VI mask = { 2, 0 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_9 (void) { VI mask = { 2, 1 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_10 (void) { VI mask = { 2, 2 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_11 (void) { VI mask = { 2, 3 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_12 (void) { VI mask = { 3, 0 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_13 (void) { VI mask = { 3, 1 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_14 (void) { VI mask = { 3, 2 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); } __attribute__((noinline, noclone)) void test_15 (void) { VI mask = { 3, 3 }; int i; c = __builtin_shuffle (a, mask); d = __builtin_shuffle (a, b, mask); __asm ("" : : "r" (&c), "r" (&d) : "memory"); for (i = 0; i < 2; ++i) if (c[i] != a[mask[i] & (2 - 1)]) abort (); else if ((mask[i] & 2)) { if (d[i] != b[mask[i] & (2 - 1)]) abort (); } else if (d[i] != a[mask[i] & (2 - 1)]) abort (); }





int
main ()
{

  int i;
  for (i = 0; i < 2; ++i)
    {
      a[i] = i + 2;
      b[i] = 2 + i + 2;
    }




  test_0 (); test_1 (); test_2 (); test_3 (); test_4 (); test_5 (); test_6 (); test_7 (); test_8 (); test_9 (); test_10 (); test_11 (); test_12 (); test_13 (); test_14 (); test_15 ();





  return 0;
}
# 16 "./torture/vshuf-v2di.c" 2
