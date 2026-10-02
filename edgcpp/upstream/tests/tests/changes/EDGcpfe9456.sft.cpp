//type:fp
//options_all:--microsoft
//remark:[4.1] Microsoft-mode explicit overriders and dependent base classes (IL CHANGE)
// 1/22/09  [EDGcpfe/9456]
//
// Microsoft-mode explicit overriders and dependent base classes (IL CHANGE)
//
// In Microsoft mode, the front end accepts a construct to indicate that a
// pure virtual function from a specific base class should be overridden (see
// Changes entry of 7/18/03).  However, the front end did not properly handle
// explicitly overriding a member of a template-dependent base class.
//
// More complex examples were prone to abort.  This is now fixed.
//
// IL CHANGE: The fix included replacing the overridden_function field of
// a_routine (a_routine*) by an overridden_functions list (an_il_entity_list*).
struct B { virtual void f() = 0; };
template<typename T> struct X: T {
  void T::f() {}  // Previously triggered an error because T was not
};                // recognized to be a base of X<T>.
