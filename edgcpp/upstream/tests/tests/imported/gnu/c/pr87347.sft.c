//type: fp
//options: --c17 --strict_gnu
/* { dg-do compile } */
/* { dg-options "-std=gnu17 -Wabsolute-value" } */

int a;
int abs();
void b() { abs(a); }
