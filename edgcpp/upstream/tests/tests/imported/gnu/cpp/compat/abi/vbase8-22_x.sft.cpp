//type: fp
//options:  -w
# 0 "./compat/abi/vbase8-22_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/abi/vbase8-22_x.C"


# 1 "./compat/abi/vbase8-22.h" 1
class C0
{ public: int i0; };
class C1
 : public C0
{ public: int i1; };
class C2
 : public C1
 , virtual public C0
{ public: int i2; };
class C3
 : virtual public C0
 , virtual public C2
 , virtual public C1
{ public: int i3; };
class C4
 : virtual public C2
 , public C1
 , virtual public C3
 , public C0
{ public: int i4; };
class C5
 : virtual public C0
 , virtual public C4
 , public C1
 , virtual public C2
 , virtual public C3
{ public: int i5; };
class C6
 : public C0
 , virtual public C1
 , public C5
 , public C2
 , virtual public C3
 , virtual public C4
{ public: int i6; };
class C7
 : virtual public C1
 , public C5
 , virtual public C6
 , virtual public C4
 , virtual public C3
 , virtual public C0
{ public: int i7; };
class C8
 : virtual public C6
 , virtual public C1
 , virtual public C2
 , public C3
 , virtual public C4
{ public: int i8; };
class C9
 : public C4
 , virtual public C2
 , virtual public C8
 , public C3
 , public C1
 , public C6
 , public C5
{ public: int i9; };
# 4 "./compat/abi/vbase8-22_x.C" 2

extern void check_C0 (C0&, int);
extern void check_C1 (C1&, int);
extern void check_C2 (C2&, int);
extern void check_C3 (C3&, int);
extern void check_C4 (C4&, int);
extern void check_C5 (C5&, int);
extern void check_C6 (C6&, int);
extern void check_C7 (C7&, int);
extern void check_C8 (C8&, int);
extern void check_C9 (C9&, int);

void
vbase8_22_x (void)
{
  C0 c0;
  C1 c1;
  C2 c2;
  C3 c3;
  C4 c4;
  C5 c5;
  C6 c6;
  C7 c7;
  C8 c8;
  C9 c9;

  c0.i0 = 0;
  c1.i1 = 101;
  c2.i2 = 202;
  c3.i3 = 303;
  c4.i4 = 404;
  c5.i5 = 505;
  c6.i6 = 606;
  c7.i7 = 707;
  c8.i8 = 808;
  c9.i9 = 909;

  check_C0 (c0, 0);
  check_C1 (c1, 101);
  check_C2 (c2, 202);
  check_C3 (c3, 303);
  check_C4 (c4, 404);
  check_C5 (c5, 505);
  check_C6 (c6, 606);
  check_C7 (c7, 707);
  check_C8 (c8, 808);
  check_C9 (c9, 909);
}
