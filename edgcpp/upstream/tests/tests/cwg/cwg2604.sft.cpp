//type:fp
//options_all:--c++20 -tused -A -W
  template<typename> [[noreturn]] void h([[maybe_unused]] int i);
  template<> void h<int>(int i) {
    // Implementations are expected not to warn that the function returns but can
    // warn about the unused parameter.
  }

//cwg: 2604
//title: Attributes for an explicit specialization
//meeting: Kona 11/22
//edg_status: EDGcpfe/25819
