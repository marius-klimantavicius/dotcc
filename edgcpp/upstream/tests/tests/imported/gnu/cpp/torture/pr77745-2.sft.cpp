//type: rp
//options: 
# 0 "./torture/pr77745-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr77745-2.C"



# 1 "./torture/pr77745.C" 1






inline void* operator new(long unsigned int, void* __p) noexcept { return __p; }

long __attribute__((noinline)) foo(char *c1, char *c2)
{
  long *p1 = new (c1) long;
  *p1 = 100;
  long long *p2 = new (c2) long long;
  *p2 = 200;
  long *p3 = new (c2) long;
  *p3 = 200;
  return *p1;
}
int main()
{
  union {
      char c;
      long l;
      long long ll;
  } c;
  if (foo(&c.c, &c.c) != 200)
    __builtin_abort();
}
# 5 "./torture/pr77745-2.C" 2
