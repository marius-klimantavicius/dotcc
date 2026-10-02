//type:fp
//options_all:--g++
//remark:[6.7] GNU C++14 compatibility: Incomplete variable template types
// 1/9/24   [EDGcpfe/26916]
//
// GNU C++14 compatibility: Incomplete variable template types
//
// In GNU C++14 mode, the front end now accepts variable templates declared with
// an incomplete class type.
struct S;
template<typename> S v;  // Ordinarily an error.  Now accepted in GNU C++14
                         // mode.
