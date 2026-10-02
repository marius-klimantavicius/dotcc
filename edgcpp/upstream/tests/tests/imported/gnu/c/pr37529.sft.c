//type: fn
//options: --c89 --strict_gnu
/* PR c/37529 */
/* { dg-do compile } */
/* { dg-options "-std=gnu89" } */

void
foo ()
{
  goto *;	/* { dg-error "expected expression before" } */
}
