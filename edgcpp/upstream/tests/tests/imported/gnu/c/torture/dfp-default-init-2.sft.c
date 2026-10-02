//type: rp
//options: 
# 0 "./torture/dfp-default-init-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/dfp-default-init-2.c"







# 1 "./torture/dfp-default-init-1.c" 1





extern void exit (int);
extern void abort (void);
void *memset (void *, int, long unsigned int);
int memcmp (const void *, const void *, long unsigned int);
# 19 "./torture/dfp-default-init-1.c"
_Decimal64 zero_int = 0;
_Decimal64 zero_fp = 0e-398DD;
_Decimal64 default_init;
_Decimal64 empty_init = {};
_Decimal64 zero_bytes;
_Decimal64 x;

struct s { _Decimal64 a, b; };
struct s s_default_init;
struct s s_empty_init = {};
struct s s_first_int = { 0 };
struct s s_both_int = { 0, 0 };
struct s sx;

const _Decimal64 a_default_init[10];
const _Decimal64 a_empty_init[10] = {};
const _Decimal64 a_first_int[10] = { 0 };
const _Decimal64 a_two_int[10] = { 0, 0 };
# 60 "./torture/dfp-default-init-1.c"
int
main (void)
{
  memset (&zero_bytes, 0, sizeof zero_bytes);
  if (memcmp (&zero_bytes, &zero_int, sizeof zero_int) == 0)
    abort ();
  do { if (memcmp (&zero_fp, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&zero_fp; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&default_init, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&default_init; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&empty_init, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&empty_init; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&s_default_init.a, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&s_default_init.a; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&s_default_init.b, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&s_default_init.b; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&s_empty_init.a, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&s_empty_init.a; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&s_empty_init.b, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&s_empty_init.b; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&s_first_int.a, &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&s_first_int.a; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  do { if (memcmp (&s_first_int.b, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&s_first_int.b; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&s_both_int.a, &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&s_both_int.a; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  do { if (memcmp (&s_both_int.b, &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&s_both_int.b; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  do { if (memcmp (&a_default_init[0], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_default_init[0]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_default_init[1], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_default_init[1]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_default_init[2], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_default_init[2]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_default_init[9], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_default_init[9]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_empty_init[0], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_empty_init[0]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_empty_init[1], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_empty_init[1]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_empty_init[2], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_empty_init[2]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_empty_init[9], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_empty_init[9]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_first_int[0], &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&a_first_int[0]; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  do { if (memcmp (&a_first_int[1], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_first_int[1]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_first_int[2], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_first_int[2]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_first_int[9], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_first_int[9]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_two_int[0], &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&a_two_int[0]; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  do { if (memcmp (&a_two_int[1], &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&a_two_int[1]; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  do { if (memcmp (&a_two_int[2], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_two_int[2]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&a_two_int[9], &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&a_two_int[9]; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  struct s s2 = {};
  do { if (memcmp (&s2.a, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&s2.a; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  do { if (memcmp (&s2.b, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&s2.b; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  struct s s3 = { 0 };
  do { if (memcmp (&s3.a, &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&s3.a; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  do { if (memcmp (&s3.b, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&s3.b; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  struct s s4 = { 0, 0 };
  do { if (memcmp (&s4.a, &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&s4.a; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  do { if (memcmp (&s4.b, &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&s4.b; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  struct s s5 = { 0 };
  sx = s5;
  do { if (memcmp (&sx.a, &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&sx.a; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  do { if (memcmp (&sx.b, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&sx.b; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  x = default_init;
  do { if (memcmp (&x, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&x; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  x = zero_int;
  do { if (memcmp (&x, &zero_int, sizeof zero_int) != 0) abort (); _Decimal64 tmp = *&x; if (memcmp (&tmp, &zero_int, sizeof zero_int) != 0) abort (); } while (0);
  x = s_default_init.a;
  do { if (memcmp (&x, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&x; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  x = s_default_init.b;
  do { if (memcmp (&x, &zero_bytes, sizeof zero_bytes) != 0) abort (); _Decimal64 tmp = *&x; if (memcmp (&tmp, &zero_bytes, sizeof zero_bytes) != 0) abort (); } while (0);
  exit (0);
}
# 9 "./torture/dfp-default-init-2.c" 2
