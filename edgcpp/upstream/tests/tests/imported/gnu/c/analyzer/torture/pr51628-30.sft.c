//type: fn
//options: 
# 0 "./analyzer/torture/pr51628-30.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/pr51628-30.c"



# 1 "./analyzer/torture/../../../c-c++-common/pr51628-30.c" 1




struct A { __complex int i; };
struct B { struct A a; };
struct C { struct B b __attribute__ ((packed)); };


extern struct C *p;

int*
foo1 (void)
{
  return &__real(p->b.a.i);

}

int*
foo2 (void)
{
  return &__imag(p->b.a.i);

}
# 5 "./analyzer/torture/pr51628-30.c" 2
