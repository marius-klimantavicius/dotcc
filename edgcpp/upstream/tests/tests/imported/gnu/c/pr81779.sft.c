//type: fp
//options: 
# 0 "./pr81779.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr81779.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 6 "./pr81779.c" 2

bool
f2 (char *p)
{
  if (!p)
    return false;

  bool ret = true;
  return ret;
}
