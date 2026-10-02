//type: fp
//options: --c89 --strict_gnu -E
/* PR preprocessor/30805 - ICE while token pasting.  */
/* { dg-do preprocess } */
/* { dg-options "-std=gnu89" } */

#define A(x,...) x##,##__VA_ARGS__
A(1)
