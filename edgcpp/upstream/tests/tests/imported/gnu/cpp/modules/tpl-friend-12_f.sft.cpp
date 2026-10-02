//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

import M;

int main() {
  B<double> b{};
  go_in_module();
}
