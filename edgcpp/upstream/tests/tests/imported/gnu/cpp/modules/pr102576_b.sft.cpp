//type: fp
//options:  --c++20 --modules
// PR c++/102576
// { dg-additional-options -fmodules-ts }

import "pr102576_a.H";

int main() {
  for (int i : {1, 2, 3})
    ;
}
