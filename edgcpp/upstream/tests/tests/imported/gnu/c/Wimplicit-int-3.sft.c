//type: fp
//options: --c17 --strict_gnu
/* { dg-do compile } */
/* { dg-options "-std=gnu17 -pedantic-errors -Wno-implicit-int" } */

static l;

foo (a)
{
  auto p;
  typedef bar;
}
