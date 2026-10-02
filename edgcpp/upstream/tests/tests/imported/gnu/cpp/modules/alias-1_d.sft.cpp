//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -isystem [srcdir] -fno-canonical-system-headers" }
// { dg-module-cmi kevin }

export module kevin;
import <alias-1_a.H>;
