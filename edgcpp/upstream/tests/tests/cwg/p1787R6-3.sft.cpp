//type:fn
//options_all:--c++20 -tused -A
template<class T, int i> class Y {
  int T;                                // error: template-parameter hidden
  void f() {
    char T;                             // error: template-parameter hidden
  }
  friend void T();                      // OK: no name bound
};

template<class X> class X;              // error: hidden by template-parameter 
