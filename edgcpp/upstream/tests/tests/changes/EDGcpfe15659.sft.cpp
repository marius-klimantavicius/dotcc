//type:fp
//options_all:--c++11
//remark:[4.10.1] Null-pointer values in non-type template arguments
// 1/27/15  [EDGcpfe/15659]
//
// Null-pointer values in non-type template arguments
//
// In C++11 mode and in Microsoft C++ modes with microsoft_version >= 1800, the
// front end now permits more cases of null pointer values in non-type template
// arguments.
template<class> struct Trait { using type = void; };

template<typename T, typename Trait<T>::type* = nullptr>
void ft(T*);

void g(double *p) {
  ft(p);  // Previously failed to match because the conversion of the
}         // default nullptr argument was not permitted.
