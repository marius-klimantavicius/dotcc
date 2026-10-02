//type:fp
//options_all:--c++11
//remark:[4.13] Abort with dependent nontype template argument
// 10/14/16 [EDGcpfe/17619,EDGcpfe/17627]
//
// Abort with dependent nontype template argument
//
// A change in version 4.12 (see EDGcpfe/17088, 7/11/16) caused the front end
// to abort with a segfault in check_for_routine_scope_variable when a
// dependent expression was passed as a nontype template argument.  This is
// now fixed.
template<typename T, T t> struct A { };
template<bool b> using B = A<bool, b>;
template<typename T> struct C {
  void f() {
    const bool b = T::x;
    B<b> bb;  // Previously caused a segfault
  }
};
