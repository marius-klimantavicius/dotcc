//type: fp
//options:  --c++20 --modules
# 0 "./modules/extern-tpl-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/extern-tpl-1_b.C"




# 1 "./modules/extern-tpl-1_a.H" 1




template <unsigned I> struct TPL
{
  int Source ()
  {
    return I;
  }
};

extern template class TPL<1>;

struct Foo
{
  TPL<1> m;

  Foo () {m.Source ();};

};

static Foo __ioinit;
# 6 "./modules/extern-tpl-1_b.C" 2

template class TPL<1>;
