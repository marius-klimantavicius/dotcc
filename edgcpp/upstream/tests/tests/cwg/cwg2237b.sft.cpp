//type:fn
//options_all:--c++17 -tused -A
  template<class T>
  struct A {
    A(int);  // OK, injected-class-name used
    ~A<T>(); // error: simple-template-id not allowed for destructor
  };
