//type: lp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
// { dg-do link }

import "tpl-spec-8_a.H";

int main() {
  A<int>::f();
}
