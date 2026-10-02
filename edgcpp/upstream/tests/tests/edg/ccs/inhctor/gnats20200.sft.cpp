//type:cp
//options:--c++17

struct A { A(int); };
struct B : A { using A::A; };

struct V1 : virtual B { using B::B; };
struct V2 : virtual B { using B::B; };

struct D2 : V1, V2 {
  using V1::V1;
  using V2::V2;
};

D2 d2(0); // OK: initializes virtual B base class, which initializes the A base class
// then initializes the V1 and V2 base classes as if by a defaulted default constructor
