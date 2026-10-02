//type: fp
//options: --c17 --strict_gnu
# 0 "./analyzer/torture/pr93379.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/pr93379.c"


# 1 "./analyzer/torture/../../torture/pr57330.c" 1



void foo (int a)
{}

void *a;
void bar ()
{
  void **( *b ) ( ) = (void**(*)()) foo;
  a = b (0);
}
# 4 "./analyzer/torture/pr93379.c" 2
