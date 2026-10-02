//options_all:--c++23 -A
  class C {
   private:
    constexpr int C(int) {}
    friend void foo(int (*a)[1]) noexcept;
  };

  constexpr int bar(C) { return 1; }

  void foo(int (&a)[bar(1)]) noexcept(bar(2) > 0); // presumably OK because of friendship

//cwg: 2715
//title: calling function for parameter initialization may not exist
//meeting: Varna 6/23
//edg_status: EDGcpfe/26817
