//type: fp
//options: --c++14
// PR c++/81942
// { dg-do compile { target c++14 } }

class A {
public:
    constexpr A() {
      return;
    }
};

A mwi;
