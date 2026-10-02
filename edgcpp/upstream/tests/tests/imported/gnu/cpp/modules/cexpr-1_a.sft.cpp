//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module Const;
// { dg-module-cmi "Const" }

export constexpr int SQ (int b)
{
  return b * b;
}
