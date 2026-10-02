//type:fp
//options_all:--c++17
//remark:[5.1] Abort when using local classes in "new" expressions inside a default template
// 7/4/19   [EDGcpfe/21445]
//
// Abort when using local classes in "new" expressions inside a default template
// argument
//
// The front end would abort with an assertion failure in scan_new_operator when
// it encountered a "new" expression using a local class when parsing a default
// template argument.
//
// This is now fixed.
template<int I = [] {
  struct A {
    int&& x = 29;
  };
  decltype(new A) x; // Previously caused an abort
  return 0;
}()>
int f();
