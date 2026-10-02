//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

struct S { typedef int I; };
struct V {
  struct I {
    unsigned int* p;
  }; 
  struct C : public S {
    void foo (const V::I& x) { x.p; } 
  };
}; 

