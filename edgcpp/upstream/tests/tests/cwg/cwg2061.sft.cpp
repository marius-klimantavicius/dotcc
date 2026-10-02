//type:fp
//options_all:--c++17 -tused -A
//
  namespace A {
    inline namespace b {
      namespace C {
        template<typename T> void f();
      }
    }
  }

  namespace A {
    namespace C {
      template<> void f<int>() { }
    }
  }

//cwg: 2061
//title: Inline namespace after simplifications
//meeting: Jacksonville 2/16
//edg_status: EDGcpfe/21999
//fixed_in: 6.5
