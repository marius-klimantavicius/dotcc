//options_all:--c++20 -tused -A
  struct S {
    void f(int);
  private:
    void f(double);
  };

  void g(S* sp) {
    sp->f(2);    // OK, access control applied after overload resolution
  }

//cwg: 2662
//title: Example for member access control vs. overload resolution
//meeting: Issaquah 2/23
//edg_status: Passes
