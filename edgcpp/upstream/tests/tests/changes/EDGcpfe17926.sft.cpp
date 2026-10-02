//type:fp
//options_all:--c++11 --gnu_version 40800
//remark:[5.1] Default constructor not correctly inherited
// 11/15/18 [EDGcpfe/17926,EDGcpfe/20176,EDGcpfe/20373,EDGcpfe/20470]
//
// Default constructor not correctly inherited
//
// The front end previously did not correctly handle user-declared default
// constructors.
//
// That is now fixed.
struct B { B(); };
struct D: B {
   using B::B;  // Did not correctly inherit B's default constructor.
   D(D&&);
};
D d{};  // Previously an error.  Now okay.
