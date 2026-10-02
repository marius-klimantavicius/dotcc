//type: fp
//options:  --c++20 --modules
// PR c++/105322
// { dg-additional-options -fmodules-ts }

import pr105322.Decltype;

int main() {
  g1();
  g2();
  g3();
}
