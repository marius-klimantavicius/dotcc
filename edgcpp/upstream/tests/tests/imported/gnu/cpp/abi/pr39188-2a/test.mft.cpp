//source_files: pr39188-2b.C
//type: rp
//options: 
# 0 "./abi/pr39188-2a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/pr39188-2a.C"





# 1 "./abi/pr39188-2.h" 1
template<typename T>
T
f (T x)
{
  static union
    {
      T i;
    };
  T j = i;
  i = x;
  return j;
}
# 7 "./abi/pr39188-2a.C" 2

int
x (int i)
{
  return f<int> (i);
}
