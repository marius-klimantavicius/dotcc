//type: fp
//options: 
# 0 "./ubsan/pr99106.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ubsan/pr99106.C"




# 1 "./ubsan/../ext/flexary38.C" 1




struct T { int t; };
struct S { char c; int T::*b[]; } a;
struct U { char c; int T::*b[0]; } b;
struct V { char c; int T::*b[1]; } c;
struct W { char c; int T::*b[2]; } d;

void
foo ()
{
  a.c = 1;
  b.c = 2;
  c.c = 3;
  d.c = 4;
}
# 6 "./ubsan/pr99106.C" 2
