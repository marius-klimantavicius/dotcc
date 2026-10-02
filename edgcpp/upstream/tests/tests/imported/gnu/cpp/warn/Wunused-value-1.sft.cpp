//type: fp
//options: --c++11
// PR c++/107797
// { dg-do compile { target c++11 } }
// { dg-options "-Wunused" }

void
g ()
{
  (long) new int{};
  long(new int{});
  (long) new int();
  long(new int());
}
