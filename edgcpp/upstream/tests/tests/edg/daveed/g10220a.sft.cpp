//remark: Hiding of using-declarations
//options:-A;fp:;fp:--g++;fp:--microsoft;fp

  struct B { template <class T> int f(T); };
  struct D: B { 
    using B::f;
    template <class T> int f(T);
  };
  void g(D *p) {
    p->f(1);  // Previously triggered a spurious ambiguity error.
  }
