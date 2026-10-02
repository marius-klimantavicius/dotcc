//options_all:-r -x -tused
//options: --strict;cn

  typedef const int T;
  T i;                         // Error -- uninitialized const variable
  struct S { const int j; };   // Warning -- no constructor
  S s;                         // Error -- uninitialized const member
  void f()
  {
    T* pi = new T;             // warning -- uninitialized const object
    S* ps = new S;             // warning -- uninitialized const member
  }

