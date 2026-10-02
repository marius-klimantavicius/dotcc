//type: fn
//options: --c++14 -W
/* { dg-do compile { target { ! c++14 } } } */
/* { dg-options "-pedantic-errors -Wvla" } */

void func (int i)
{
  int array[i]; /* { dg-error "7:ISO C.* forbids variable.* array 'array'" } */
}
