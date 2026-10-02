//type: fp
//options: 
# 0 "./lto/inline-crossmodule-1_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/inline-crossmodule-1_0.C"


# 1 "./lto/inline-crossmodule-1.h" 1
struct a
{
  int ret1 ()
  {
    return 1;
  }
  int key ();
};
struct b
{
  int ret2 ()
  {
    return 2;
  }
};
# 4 "./lto/inline-crossmodule-1_0.C" 2
int a::key ()
{
  return 0;
}
