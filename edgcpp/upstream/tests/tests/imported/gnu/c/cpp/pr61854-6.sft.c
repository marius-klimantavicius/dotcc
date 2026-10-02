//type: rp
//options: --c89 --strict_gnu
/* PR c/61854 */
/* { dg-do run } */
/* { dg-options "-std=gnu89" } */

int
main (void)
{
  int i = 0
#if 0
// /*
#else
// */
+1
#endif
;
  if (i != 1)
    __builtin_abort ();
  return 0;
}
