//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module hello;
// { dg-module-cmi hello }

import "binding-1_a.H";
import "binding-1_b.H";
