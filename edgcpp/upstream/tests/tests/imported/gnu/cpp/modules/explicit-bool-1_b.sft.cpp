//type: fp
//options:  --c++20 --modules --c++20
// { dg-additional-options -fmodules-ts }
// { dg-require-effective-target c++20 }

export module x;
import "explicit-bool-1_a.H";
pair<string, string> environment;
