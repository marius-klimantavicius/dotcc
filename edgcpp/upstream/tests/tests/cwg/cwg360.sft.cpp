//type:fn
//options_all:--c++20 -tused -A

  struct A {
    int foo ();
  };

  struct B: A {
  private:
    using A::foo;
  };

  int main ()
  {
    return B ().foo ();
  }

//cwg: 360
//title: Using-declaration that reduces access
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23824
