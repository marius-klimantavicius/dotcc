//type: fn
//options: --c89 --strict_gnu
/* { dg-do compile } */
/* { dg-options "-std=gnu89" } // suppress default -pedantic-errors */

int f()
{
  struct f {
  }
  int z; /* { dg-error "expected ';', identifier or " } */
}
