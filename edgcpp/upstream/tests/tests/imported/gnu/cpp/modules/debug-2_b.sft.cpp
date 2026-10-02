//type: fp
//options:  --c++20 --modules
// PR c++/112820
// { dg-additional-options "-fmodules-ts -g" }

module io;

const char* error::what() const noexcept {
  return "bla";
}
