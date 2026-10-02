//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

// From ARM 11.7
class W {public: void f();};
class A : private virtual W {};
class B : public virtual W {};
class C : public A, public B {
  void f() {W::f();} // okay
};

