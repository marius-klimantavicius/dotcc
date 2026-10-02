//type: rp
//options: 
# 0 "./torture/pr85588.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr85588.c"



# 1 "./torture/pr57656.c" 1



int main (void)
{
  int a = -1;
  int b = 0x7fffffff;
  int c = 2;
  int t = 1 - ((a - b) / c);
  if (t != (1 - (-1 - 0x7fffffff) / 2))
    __builtin_abort();
  return 0;
}
# 5 "./torture/pr85588.c" 2
