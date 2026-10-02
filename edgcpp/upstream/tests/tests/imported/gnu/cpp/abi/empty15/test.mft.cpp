//source_files: empty15a.c
//type: rp
//options:  -w
# 0 "./abi/empty15.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/empty15.C"






# 1 "./abi/empty15.h" 1
struct A1 {};
struct A2 {};
struct B1 { struct A1 a; struct A2 b; };
struct B2 { struct A1 a; struct A2 b; };
struct C1 { struct B1 a; struct B2 b; };
struct C2 { struct B1 a; struct B2 b; };
struct D1 { struct C1 a; struct C2 b; };
struct D2 { struct C1 a; struct C2 b; };
struct E1 { struct D1 a; struct D2 b; };
struct E2 { struct D1 a; struct D2 b; };
struct F1 { struct E1 a; struct E2 b; };
struct F2 { struct E1 a; struct E2 b; };
struct G1 { struct F1 a; struct F2 b; };
struct G2 { struct F1 a; struct F2 b; };
struct H1 { struct G1 a; struct G2 b; };
struct H2 { struct G1 a; struct G2 b; };
struct I1 { struct H1 a; struct H2 b; };
struct I2 { struct H1 a; struct H2 b; };
struct J1 { struct I1 a; struct I2 b; };
struct J2 { struct I1 a; struct I2 b; };
struct dummy { struct J1 a; struct J2 b; };

struct foo
{
  int i1;
  int i2;
  int i3;
  int i4;
  int i5;
};
# 8 "./abi/empty15.C" 2
extern "C" void fun(struct dummy, struct foo);

int main()
{
  struct dummy d;
  struct foo f = { -1, -2, -3, -4, -5 };

  fun(d, f);
  return 0;
}
