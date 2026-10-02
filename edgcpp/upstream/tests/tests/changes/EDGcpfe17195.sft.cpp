//type:fp
//options_all:--c++11
//remark:[4.12] Base-class casts in constant expressions
// 8/9/16   [EDGcpfe/17195]
//
// Base-class casts in constant expressions
//
// The changes for EDGcpfe/17117 (in version 4.11) introduced a regression in
// which the front end reported spurious errors when a derived class glvalue
// is cast to a base class prvalue.  This is now fixed.
// --c++11:
struct base {
  constexpr base(int l) {}
  constexpr base() {}
};
struct derived : public base {
  constexpr derived() {}
  constexpr derived(const derived& d) : base(d) {}
};
constexpr derived s;
constexpr derived t(s);  // Previously a spurious "must be constant" error
