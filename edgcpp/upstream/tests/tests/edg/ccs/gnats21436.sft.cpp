//type:fn
//options::--microsoft_version 1916;cp:--microsoft_version 1916 --no_ms_permissive
//options_all:--c++11

using T = int;
T t{};
const volatile T& x = static_cast<T&&>(t);
