//remark:Null pointer nontype template arguments
//options:--microsoft_version=1927;fn:--microsoft_version=1928;fp

template<int i> int f_() { return 1; }
template<int* i> int f_() { return 2; }

// constant expr that evaluates to a null pointer value
// _CXX11 - Implements core 354 - 2006
int a = f_<0>();
