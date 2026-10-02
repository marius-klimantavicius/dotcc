//type: fp
//options: --c++11
// PR c++/52845
// { dg-do compile { target c++11 } }
// { dg-options "-Wall" }

void f()
{
  [](){};
}
