//type: fp
//options:  -w
# 0 "./compat/abi/vbase8-21_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/abi/vbase8-21_y.C"


extern "C" void abort (void);

# 1 "./compat/abi/vbase8-21.h" 1
class C0
{ public: int i0; };
class C1
 : virtual public C0
{ public: int i1; };
class C2
 : virtual public C1
 , virtual public C0
{ public: int i2; };
class C3
 : virtual public C2
 , virtual public C1
{ public: int i3; };
class C4
 : virtual public C2
 , public C0
 , public C1
{ public: int i4; };
class C5
 : virtual public C0
 , public C2
 , virtual public C1
 , virtual public C3
 , virtual public C4
{ public: int i5; };
class C6
 : virtual public C1
 , virtual public C3
 , public C0
 , public C2
 , virtual public C4
{ public: int i6; };
class C7
 : virtual public C5
 , public C2
 , public C6
 , virtual public C0
 , public C3
{ public: int i7; };
class C8
 : virtual public C5
 , public C7
 , virtual public C0
 , virtual public C2
 , virtual public C6
{ public: int i8; };
class C9
 : virtual public C2
 , virtual public C4
 , public C1
 , virtual public C0
 , public C7
 , public C5
{ public: int i9; };
# 6 "./compat/abi/vbase8-21_y.C" 2

void check_C0 (C0 &x, int i)
{
  if (x.i0 != i)
    abort ();
}

void check_C1 (C1 &x, int i)
{
  if (x.i1 != i)
    abort ();
}

void check_C2 (C2 &x, int i)
{
  if (x.i2 != i)
    abort ();
}

void check_C3 (C3 &x, int i)
{
  if (x.i3 != i)
    abort ();
}

void check_C4 (C4 &x, int i)
{
  if (x.i4 != i)
    abort ();
}

void check_C5 (C5 &x, int i)
{
  if (x.i5 != i)
    abort ();
}

void check_C6 (C6 &x, int i)
{
  if (x.i6 != i)
    abort ();
}

void check_C7 (C7 &x, int i)
{
  if (x.i7 != i)
    abort ();
}

void check_C8 (C8 &x, int i)
{
  if (x.i8 != i)
    abort ();
}

void check_C9 (C9 &x, int i)
{
  if (x.i9 != i)
    abort ();
}
