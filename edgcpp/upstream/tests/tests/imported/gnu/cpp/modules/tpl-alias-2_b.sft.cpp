//type: fp
//options:  --c++20 --modules
// PR c++/103994
// { dg-additional-options -fmodules-ts }

import "tpl-alias-2_a.H";

int main() {
  B b;
  b.f<int>();
}
