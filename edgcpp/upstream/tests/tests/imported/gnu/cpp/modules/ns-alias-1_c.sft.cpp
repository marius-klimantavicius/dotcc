//type: fn
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

import foo;

elsewhere::det::bob j;
elsewhere::ail::bob k; // { dg-error "does not name a type" }
