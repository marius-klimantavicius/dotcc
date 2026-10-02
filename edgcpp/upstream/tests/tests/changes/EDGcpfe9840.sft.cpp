//type:fp
//options_all:--microsoft
//remark:[4.1] Microsoft compatibility: CV-qualified parameter types and virtual overriding
// 5/26/09  [EDGcpfe/9840]
//
// Microsoft compatibility: CV-qualified parameter types and virtual overriding
//
// In Microsoft bugs mode, the front end previously considered top-level parameter
// type qualifiers when determining whether a function in a derived class
// overrides a virtual function in a base class (see Changes entry of 9/18/97).
// This is now no longer done if microsoft_version >= 1500.
struct B { virtual void f(int) = 0; };
struct D: B {
  void f(int const);  // Overrides B::f(int) when microsoft_version >= 1500,
};                    // but not when microsoft_version < 1500 (in Microsoft
                      // bugs mode).
