//type:cp
//options:--c++11:--microsoft_version 1936

struct A {
  class B {
    int m_0{0};
  };
  B b1{};
  B b2{}; // only gives an error on this declaration
};

A a;
