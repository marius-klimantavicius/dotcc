//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

// Constructor with default args that use temporaries used to initialize
// an array.
  extern "C" int printf(char *, ...);
int Tseed = 0;
  struct T {
    int i;
    T() : i(++Tseed)  { printf("T::T(%d)\n", i); }
    ~T() { printf("T::~T(%d)\n", i); }
  };
  struct S {
    int i;
    S( const T&p = T() ) : i(p.i) { printf("S::S(%d)\n", i); }
    ~S() { printf("S::~S(%d)\n", i); }
  };
  S x;
  S y[2];
  main() { }


