//type: fp
//options: 
# 0 "./dfp/union-init.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/union-init.c"


# 1 "./dfp/dfp-dbg.h" 1


int failures;
# 4 "./dfp/union-init.c" 2

union u
{
  _Decimal128 d128;
  double d;
};

union n
{
  double d;
  _Decimal64 d64;
};

int main ()
{
  static union u u1 = { 0.0dl };
  static union u u2 = { 4.2dl };
  static union u u4 = { 0.0 };

  static union n n1 = { 2.2dd };
  static union n n2 = { 3.25 };

  _Decimal128 d128;
  _Decimal64 d64;
  double d;

  if (u1.d128 != 0.0dl)
    __builtin_abort ();

  if (u2.d128 != 4.2dl)
    __builtin_abort ();


  d128 = 1.23dl;
  d64 = 4.56dd;
  d = 3.25;

  u4 = (union u) d128;
  if (u4.d128 != 1.23dl)
    __builtin_abort ();

  u4 = (union u) d;
  if (u4.d != 3.25)
    __builtin_abort ();

  n1 = (union n) d64;
  if (n1.d64 != 4.56dd)
    __builtin_abort ();

  n1 = (union n)d;
  if (n1.d != 3.25)
    __builtin_abort ();

  return 0;
}
