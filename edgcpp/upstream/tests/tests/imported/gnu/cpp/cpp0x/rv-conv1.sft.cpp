//type: fp
//options: --c++11
// PR c++/49267
// { dg-do compile { target c++11 } }

struct X {
  operator int&();
  operator int&&();
};

int&&x = X();
