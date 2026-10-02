//type: fp
//options: --c++14
// { dg-do compile { target c++14 } }

union U
{
  char *x = &y;
  char y;
};

constexpr U u = {};
