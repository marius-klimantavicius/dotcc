//type: fp
//options:  -w
# 0 "./compat/break/bitfield7_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/bitfield7_y.C"


extern "C" void abort (void);

# 1 "./compat/break/bitfield7.h" 1
union U {
  int i: 4096;
};
# 6 "./compat/break/bitfield7_y.C" 2

void bitfield7_y (U* u)
{
  if (u[0].i != 7)
    abort ();
  if (u[1].i != 8)
    abort ();
}
