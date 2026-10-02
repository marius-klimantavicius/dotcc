//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;cp

  // file a.c
  class A {
  public:
    static int zero() { return 0; }
  };
  
  // file b.c
  template <class T> int f(T*) {
    return T::zero();
  };

