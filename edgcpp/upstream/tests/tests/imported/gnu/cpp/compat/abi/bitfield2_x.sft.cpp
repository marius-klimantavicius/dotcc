//type: fp
//options:  -w --c++03 -W
# 0 "./compat/abi/bitfield2_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/abi/bitfield2_x.C"


# 1 "./compat/abi/bitfield1.h" 1
typedef int Int;
typedef signed int SInt;
typedef unsigned int UInt;

struct A
{
  SInt bitS : 1;
  UInt bitU : 1;
  Int bit : 1;
};
# 4 "./compat/abi/bitfield2_x.C" 2

extern void bitfield1_y (A& a);

void bitfield1_x ()
{
  A a;

  a.bitS = 1;
  a.bitU = 1;
  a.bit = 1;

  bitfield1_y (a);
}
