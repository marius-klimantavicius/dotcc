//type:rp
//options_all:--c++23 -A 
//remark: Test passes, but output is not correct
//Expected outcome
//get 0
//get 1
//~X
//done
// ~B 1
// ~ B 0
// ~C
extern "C" int printf(const char *, ...);

  struct X
  {
    ~X()
    {
      printf("~X\n");
    }
  };

  struct C
  {
    C(const X &)
    { }

    ~C()
    {
      printf("~C\n");
    }
  };

  namespace std
  {
    template<typename T>
    struct tuple_size
    { };

    template<int I, typename T>
    struct tuple_element
    { };
  }

  template<>
  struct std::tuple_size<C>
  {
    static constexpr int value = 2;
  };

  template<int I>
  struct B
  {
    ~B()
    {
      printf("~B %d\n", I);
    }
  };

  template<int I>
  struct std::tuple_element<I, C>
  {
    using type = B<I>;
  };

  template<int I>
  B<I> get(const C &)
  {
    printf("get %d\n", I);
    return { };
  }

  int main()
  {
    auto [ b1, b2 ] = C(X());
    printf("done\n");
  }

//cwg: 2867
//title: Order of initialization for structured bindings
//meeting: St Louis 6/24
//edg_status: EDGcpfe/27419
