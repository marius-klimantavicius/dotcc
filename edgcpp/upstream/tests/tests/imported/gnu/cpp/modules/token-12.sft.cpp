//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

#define bob() fred
export module bob;

// { dg-module-cmi bob }
