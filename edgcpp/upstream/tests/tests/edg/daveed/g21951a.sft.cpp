//remark:__is_same_as
//options:--gnu=70300 --c++14;fp:--gnu=60300 --c++14;fn

template < typename _Tp, _Tp > struct integral_constant;
template < typename _Tp, typename _Up >
struct is_same:integral_constant < int, __is_same_as (_Tp, _Up) > {};
