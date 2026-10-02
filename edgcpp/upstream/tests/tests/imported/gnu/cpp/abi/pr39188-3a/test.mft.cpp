//source_files: pr39188-3b.C
//type: rp
//options: 
# 0 "./abi/pr39188-3a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/pr39188-3a.C"





# 1 "./abi/pr39188-3.h" 1
static int
f (int x)
{
  static union
    {
      int i;
    };
  int j = i;
  i = x;
  return j;
}
# 7 "./abi/pr39188-3a.C" 2

int
x (int i)
{
  return f (i);
}
