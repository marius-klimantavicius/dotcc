//type: fp
//options: --c++11
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
// PR c++/47388
// { dg-do compile { target c++11 } }
// { dg-options "-fno-for-scope -Wno-deprecated" }

template <int>
void
foo ()
{
  int a[] = { 1, 2, 3, 4 };
  for (int i : a)
    ;
}

void
bar ()
{
  foo <0> ();
}
