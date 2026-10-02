//type:fp
//options_all:--c++17
//remark:[6.6] C++-generating back end: crash with non-public name as template argument
// 6/21/23  [EDGcpfe/26262]
//
// C++-generating back end: crash with non-public name as template argument
//
// In C++-generating back end configurations, the front end sometimes entered
// an unbounded recursion when attempting to put out a template-id in which a
// template argument refers to a non-public class member.  This is now fixed.
//
// -------------------------------------------------------------------------------
template <typename T> struct A { using type = T; };
template <typename T> using B = typename A<T>::type;
template <int> struct S {};
class C {
  static constexpr int I = 0;   // private
  using SI = S<I>;
  void f() {
    B<SI>{};                    // resulted in crash with unbounded recursion
  }
};
