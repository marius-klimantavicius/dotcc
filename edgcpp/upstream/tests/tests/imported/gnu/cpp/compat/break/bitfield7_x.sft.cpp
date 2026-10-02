//type: fp
//options:  -w
# 0 "./compat/break/bitfield7_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/break/bitfield7_x.C"


# 1 "./compat/break/bitfield7.h" 1
union U {
  int i: 4096;
};
# 4 "./compat/break/bitfield7_x.C" 2

extern void bitfield7_y (U*);

void bitfield7_x ()
{
  U u[2];

  u[0].i = 7;
  u[1].i = 8;

  bitfield7_y (u);
}
