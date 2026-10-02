//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

module foo:imp;
// { dg-module-cmi foo:imp }

import :inter; // ok
