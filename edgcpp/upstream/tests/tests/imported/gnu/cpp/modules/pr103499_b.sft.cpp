//type: fp
//options:  --c++20 --modules
// PR c++/103499
// { dg-additional-options "-fmodules-ts" }

import pr103499;

void test(derived* p) {
  delete p;
}
