//type:fp
//options_all:--c++20
//remark:[5.1] Rvalue objects and const-qualified pointers to members
// 11/27/18 [EDGcpfe/19999,EDGcpfe/20422]
//
// Rvalue objects and const-qualified pointers to members
//
// As described in Committee document P0704R1, C++20 relaxes the rule
// prohibiting use of an rvalue object expression in a pointer-to-member
// expression where the member pointer has an lvalue ref-qualifier.  Under the
// new rules, such an expression is permitted if the member pointer has a
// const qualifier.
struct X { void foo() const &; };
void f() {
  (X{}.*&X::foo)(); // Previously an error, now accepted in C++20 because
                    // X::foo has a const qualifier
}
