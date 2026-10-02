//remark:Mal-formed enumeration definitions
//options:--c++11;fn:--c++11 -A;fn:;fn:-A;fn:--microsoft_v=1600 --c++11;fn:--microsoft_v=1600;fn

using T = enum {t1};
T x = t1;

using U = enum : class { u1 } ; 
U y = U::u1;

