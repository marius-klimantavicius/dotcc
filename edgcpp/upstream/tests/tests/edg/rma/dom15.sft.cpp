//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;cn

// EDGqa00672
//
//           A{f}
//          / \
//         /   \
//        B{f}  B{f}
//        |     |
//        C1{f} C2
//         \   /
//          \ /
//           D

extern "C" void printf(char *, ...);

struct A { 
  virtual void f()  { printf("A::f\n"); }
//  class N { };
};

struct B : public virtual A { 
  virtual void f()  { printf("B::f\n"); }
//  class N { };
};

// If B is changed to virtual derivation, the error goes away
struct C1 : public B {  
  virtual void f()  { printf("C1::f\n"); }
//  class N { };
};

struct BB : public virtual A { 
  virtual void f()  { printf("B::f\n"); }
//  class N { };
};

struct C2 : public virtual BB { };

struct D : public virtual C2 , public virtual C1 {
//  N n;
} d;

main() {
  A *pa = &d;
  pa->f();
}

