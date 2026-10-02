//type: fp
//options:  --c++20 --modules
// PR c++/119608
// { dg-additional-options "-fmodules" }

import repro;

int main() {
  visit(123);
}
