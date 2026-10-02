//type: fn
//options:  --c++20 --modules
// PR c++/110808
// { dg-additional-options "-fmodules-ts" }

import group:tres;  // { dg-error "expected .;." }

int main() {
  return mul();  // { dg-error "not declared" }
}
