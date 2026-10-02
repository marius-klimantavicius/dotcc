//type: fp
//options:  --c++20 --modules
// PR c++/115757
// { dg-additional-options "-fmodules-ts -Wunused" }

import test;

int main() {
  foo(0);
}
