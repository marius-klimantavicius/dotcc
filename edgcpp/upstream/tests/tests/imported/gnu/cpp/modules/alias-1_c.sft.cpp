//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -isystem [srcdir]" }
// { dg-module-cmi bob }

export module bob;
import "alias-1_a.H";
