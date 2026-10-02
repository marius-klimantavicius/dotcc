//type: fp
//options:  --c++03 --no_strict_gnu
// { dg-options "-std=gnu++98 -pedantic" }
template<typename... Args> class tuple; // { dg-warning "variadic templates" }
