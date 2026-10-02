//type:fp
//options_all:--c++14 -tused -A
  template<typename T> extern const T var; // declaration
  template<typename T> constexpr T var = 123; // definition

//cwg: 1712
//title: constexpr variable template declarations
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
