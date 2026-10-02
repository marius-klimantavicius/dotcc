//type:fn
//options_all:--c++17 -tused -A

  template<class T>
  struct C {
    void f() { T x; }
    void g() = delete;
  };
  C<void> c;                       // OK, definition of C<void>::f is not instantiated at this point
  template<> void C<int>::g() { }  // error: redefinition of C<int>::g

//cwg: 2260
//title: Explicit specializations of deleted member functions
//meeting: Jacksonville 2/18
//edg_status: EDGcpfe/21956
