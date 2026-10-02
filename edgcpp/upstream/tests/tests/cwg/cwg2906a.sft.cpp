//options_all:--c++20 -A
struct S {};
S a;
constexpr S b = a;        // OK, call to implicitly-declared copy constructor
constexpr S d = false ? S{} : a; // OK now

//cwg: 2906
//title: Lvalue-to-rvalue conversion of class types for conditional operator
//meeting: Wroclaw 11/24
//edg_status: EDGcpfe/27756
