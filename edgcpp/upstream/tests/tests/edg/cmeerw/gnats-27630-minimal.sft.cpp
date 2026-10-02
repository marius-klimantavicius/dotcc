//type:fp
//options:--c++ -A

  namespace ns {
    template<typename> class A;
  }
  using namespace ns;
  class C {
    template<typename>
    friend class ::A;
  };
