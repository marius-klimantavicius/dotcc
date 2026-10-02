//type: fn
//options:  --c++03 -W -E
/* { dg-do preprocess } */
/* { dg-options "-std=c++98 -pedantic-errors" } */

#if 1   
#endif 1 /* { dg-error "extra tokens at end of '#endif' directive" } */
