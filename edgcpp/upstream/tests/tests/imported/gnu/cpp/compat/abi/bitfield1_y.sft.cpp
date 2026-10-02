//type: fp
//options:  -w --c++03 -W
# 0 "./compat/abi/bitfield1_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/abi/bitfield1_y.C"


extern "C" void abort (void);

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
# 6 "./compat/abi/bitfield1_y.C" 2

void bitfield1_y (A& a)
{
  if (a.bitS != -1)
    abort ();
  if (a.bitU != 1)
    abort ();
  if (a.bit != 1)
    abort ();
}
