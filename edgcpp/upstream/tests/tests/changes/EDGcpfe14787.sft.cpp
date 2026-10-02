//type:fp
//options_all:--c++11
//remark:[4.9] Accessing of members of objects of the class type being defined
// 1/17/14  [EDGcpfe/14787]
//
// Accessing of members of objects of the class type being defined
//
// The changes for EDGcpfe/11746,EDGcpfe/11557 (entry of 9/14/11) enable accessing
// members of *this in member declarations of a class C being defined (i.e., even
// though the class is still incomplete).  Other implementations have interpreted
// the standard as also allowing member access on other expressions of a class
// type that is being defined.  The front end now uses this broader interpretation
// in its nonstrict C++11 modes.
template<class T> struct S {
  auto f()->int;
  auto g(S &b)->decltype(b.f());  // Previously an error (b has incomplete
};                                // type); now accepted in nonstrict C++11
                                  // modes.
