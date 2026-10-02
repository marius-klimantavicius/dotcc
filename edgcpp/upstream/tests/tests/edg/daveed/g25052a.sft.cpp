
//remark:Deduction guide and CTAD rendering
//options:--c++17;fp

// for EDGcpfe/25052
template<typename...> struct Keys {
  template<typename... Vs> struct Vals {
    Vals(Vs const &... args) {}
  };
  template<typename... Vs> Vals(Vs const&...) -> Vals<Vs...>;
    // Previously rendered twice.
};
void g() {
  (Keys<char, void>::Vals(1, 2)); // Previously dropped the "<char, void>".
}
