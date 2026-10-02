//remark:Explicit specialization of direct-initialized variable template
//options:--c++17;fp

  #include <initializer_list>
  template<typename> constexpr auto &v{"x"};
  template<> inline constexpr auto &v<bool>{"y"};  // Previously an error.
                                                   // Now okay.

