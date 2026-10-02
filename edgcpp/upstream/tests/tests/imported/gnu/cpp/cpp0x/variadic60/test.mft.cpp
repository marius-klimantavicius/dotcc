//type: fn
//options:  --c++11
template<typename... Args> class tuple; // { dg-error "variadic templates" "" { target { ! c++11 } } }
