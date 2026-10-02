//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module foo:inter2;
// { dg-module-cmi foo:inter2 }

import :imp; // ok

import :inter; // ok at this point
