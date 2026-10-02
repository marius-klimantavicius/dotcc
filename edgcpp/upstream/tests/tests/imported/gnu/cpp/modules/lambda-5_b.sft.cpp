//type: fp
//options:  --c++20 --modules
// PR c++/105322
// { dg-additional-options -fmodules-ts }

import pr105322.Lambda;

int main() {
  f1();
  f2();
  g3();
}
