//type:cp
//options:--c++11:--c++17:--gnu_version 90300:--clang_version 80000

struct Base { Base(int val) {} };

struct Derived : public Base {
  using Derived::Base::Base;
};

Derived x(1);
