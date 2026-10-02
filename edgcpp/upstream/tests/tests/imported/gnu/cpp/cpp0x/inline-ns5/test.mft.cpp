//type: fn
//options:  --c++03 --no_strict_gnu -W
// { dg-options "-std=gnu++98 -pedantic-errors" }
inline namespace { } // { dg-error "inline namespaces" }
