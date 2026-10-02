//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

module M;

int go_in_module() {
  return bar(B<int>{}, A<int>{});
}
