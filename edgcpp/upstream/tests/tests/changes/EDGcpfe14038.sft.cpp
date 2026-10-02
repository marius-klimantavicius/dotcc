//type:fp
//options_all:--microsoft
//remark:[4.7] Spurious error on use of type traits helper in Microsoft mode
// 5/15/13  [EDGcpfe/14038]
//
// Spurious error on use of type traits helper in Microsoft mode
//
// Version 4.5 introduced a change that caused certain nonreal classes used as
// base classes to be have actual instantiations done on them in Microsoft mode
// (see entry of 5/15/12 for EDGcpfe/12786).  This could result in spurious
// errors in certain uses of type traits helpers that require a complete type.
//
// This is now fixed.
template<bool V> struct S {};
template<typename, typename T>
  struct X : public S<!__is_abstract(T)> {};
struct I;
template<typename T>
  struct R : X<T, I> {};  // Previously triggered an error because
                          // __is_abstract is applied to the incomplete
                          // type I.  Now accepted.
