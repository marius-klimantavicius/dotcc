//type: fn
//options: --c99 --strict_gnu
/* { dg-do compile } */
/* { dg-options "-std=gnu99" } */

int foo(int i);

void bar()
{
  __complex__ int i;
  X j;			/* { dg-error "unknown" } */
  if (i = foo(j))
    ;
}
