//type:fp
//options_all:--c++14
//remark:[6.7] Abort on generic lambda with function parameter pack that is an expansion of an
// 12/19/23 [EDGcpfe/26775]
//
// Abort on generic lambda with function parameter pack that is an expansion of an
// outer pack
//
// Previously, the front end failed to correctly expand the elements of a function
// parameter pack that is an expansion of an outer pack, resulting in a failed
// assertion in scan_function_body (func_def.c).
template<typename ... T>
auto f() {
  return [](auto, T ...) { };  // Previously triggered an abort.  Now okay.
}
auto l = f<short, int>();
