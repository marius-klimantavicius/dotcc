//type: fp
//options: --c99 --strict_gnu
/* PR c/22393 */
/* { dg-options "-O -std=gnu99" } */

__complex__ double foo (__complex__ double x)
{
    return 1.0 / x * -1.0i;
}
