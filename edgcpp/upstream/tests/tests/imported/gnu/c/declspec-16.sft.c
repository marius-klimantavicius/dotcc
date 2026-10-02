//type: fp
//options: --c89 --strict_gnu
/* Test diagnostic for empty declarations in old-style parameter
   declarations.  Test with -pedantic.  */
/* Origin: Joseph Myers <joseph@codesourcery.com> */
/* { dg-do compile } */
/* { dg-options "-std=gnu89 -pedantic" } */

void
f (a, b)
     int; /* { dg-warning "empty declaration" } */
     register; /* { dg-warning "empty declaration" } */
{
}
