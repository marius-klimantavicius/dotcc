//type:fp
//options_all:--c++20
//remark:[6.5] Spurious errors for class template argument deduction for aggregates
// 1/17/23  [EDGcpfe/25343,EDGcpfe/25363,EDGcpfe/25365,EDGcpfe/25366,
//           EDGcpfe/25585]
//
// Spurious errors for class template argument deduction for aggregates
//
// Various aspects of this language feature, such as non-trailing parameter packs,
// brace elision, designated initializers, and deduction from string literals,
// were not handled correctly.
struct B { int i; int j; };

template<typename T, typename ... U>
struct C : U ... {
  B b;
  T t;
};
C c{ 1, 2, 'c' };  // Previously a deduction failure.
                   // Now deduced as C<char>.

template<typename T, int N>
struct D {
  T arr[N];
};
D d{ .arr = "Hello" };  // Previously a deduction failure.
                        // Now deduced as C<char, 6>.
