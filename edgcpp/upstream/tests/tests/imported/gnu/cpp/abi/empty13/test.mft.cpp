//source_files: empty13a.c
//type: rp
//options:  -w
# 0 "./abi/empty13.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/empty13.C"






# 1 "./abi/empty13.h" 1
struct dummy { };
struct foo
{
  int i1;
  int i2;
  int i3;
  int i4;
  int i5;
};
# 8 "./abi/empty13.C" 2
extern "C" void fun(struct dummy, struct foo);

int main()
{
  struct dummy d;
  struct foo f = { -1, -2, -3, -4, -5 };

  fun(d, f);
  return 0;
}
