//type: fp
//options: --c11 --strict_gnu -E
/* Test #warning not in C11.  */
/* { dg-do preprocess } */
/* { dg-options "-std=gnu11" } */

#warning example text /* { dg-warning "example text" } */
/* Not diagnosed by default.  */
