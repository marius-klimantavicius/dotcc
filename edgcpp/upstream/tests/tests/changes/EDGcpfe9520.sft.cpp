//type:fp
//options_all:--microsoft
//remark:[4.1] Microsoft compatibility: Explicit template arguments on function templates
// 2/11/09  [EDGcpfe/9520]
//
// Microsoft compatibility: Explicit template arguments on function templates
//
// In Microsoft mode, the front end now ignores (with a warning) explicit
// template arguments that appear on a non-member function template
// redeclaration.
template<typename T> void f();
template<typename T> void f<T>();      // <T> ignored with a warning.
template<typename T> void f<int*>() {} // <int*> ignored with a warning;
                                       // this defines the previously
                                       // declared template.
