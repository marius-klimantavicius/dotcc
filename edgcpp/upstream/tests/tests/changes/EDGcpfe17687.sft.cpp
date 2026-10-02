//type:fp
//options_all:--c++17
//remark:[6.1] C++17 inheriting constructors
// 3/13/20  [EDGcpfe/17687]
//
// C++17 inheriting constructors
//
// C++ Committee document P0136R1 (adopted in C++17) significantly changes how
// inheriting constructors are handled.  This significantly changes both the code
// that is accepted by the front end as well as how objects are initialized when
// an inheriting constructor is called.
//
// This is now implemented.
struct A { A(int); };
struct B : A { using A::A; };
struct V1 : virtual B { using B::B; };
struct V2 : virtual B { using B::B; };
struct D2 : V1, V2 {
  using V1::V1;
  using V2::V2;
};
D2 d2(0); // Now OK: initializes virtual B base class, which initializes the
          // A base class, then initializes the V1 and V2 base classes as if
          // by a defaulted default constructor
