//options_all:-r -x -tused
//options: --strict;cn

// Declaring an operator void
struct B { };
struct A : public B {
  operator int();
  operator const int();
  operator void();
  operator const void();
  operator void&();
  operator const void&();
  operator A();
  operator const A();
  operator A&();
  operator const A&();
  operator B();
  operator const B();
  operator B&();
  operator const B&();
};

