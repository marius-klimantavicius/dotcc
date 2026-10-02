//remark:Dependent destructors in constant expressions
//options:--gnu=80300;fp

template<bool b> struct integral_constant {};
template<typename Tp> Tp declval() ;

template<typename _Tp>
static integral_constant<(declval<_Tp&>().~_Tp())> foo();
