//type: fp
//options: 
# 0 "./ipa/pure-const-3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ipa/pure-const-3.C"



# 1 "./ipa/pure-const-3.h" 1
int *ptr;
static int barvar;
static int b(int a);


inline
__attribute__ ((noinline))
int a(int a)
{
  if (a>0)
    return b(a-1);
  return *ptr == *ptr;
}
inline
__attribute__ ((noinline))
static int b(int p)
{
  if (p<0)
    return a(p+1);
  return 1;
}
int main()
{
  int aa;
  ptr = &barvar;
  aa=!b(3);
  ptr = 0;
  return aa;
}
# 5 "./ipa/pure-const-3.C" 2
