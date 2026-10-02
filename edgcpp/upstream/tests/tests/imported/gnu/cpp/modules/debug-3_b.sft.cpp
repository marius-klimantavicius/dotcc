//type: fp
//options:  --c++20 --modules
// PR c++/102607
// { dg-module-do link }
// { dg-additional-options "-fmodules-ts -g" }

import mod;
int main() {
  struct D : B {};
  (void)D{};
}
