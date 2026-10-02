//type:fn
//options_all:--microsoft_version 1922
using T = int;
T t{};
const volatile T& x = static_cast<T&&>(t); // ill-formed per N4810 [dcl.init.ref]/5.2
