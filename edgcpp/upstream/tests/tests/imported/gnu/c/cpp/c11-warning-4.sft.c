//type: fp
//options: --c11 -E
/* Test #warning not in C11.  */
/* { dg-do preprocess } */
/* { dg-options "-std=c11" } */

#warning example text /* { dg-warning "example text" } */
/* Not diagnosed by default.  */
