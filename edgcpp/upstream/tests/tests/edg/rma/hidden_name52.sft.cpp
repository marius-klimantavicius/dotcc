//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

struct I { };
struct V {
  struct I {
    unsigned int* p;
  }; 
  struct C : public ::I {
    void f(V::I& x) { x.p; } 
  };
}; 

