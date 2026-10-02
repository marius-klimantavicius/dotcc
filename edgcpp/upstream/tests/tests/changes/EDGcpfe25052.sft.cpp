//type:fp
//options_all:--gn 110100 --c++17
//remark:[6.4] C++-generating back end: Deduction guides
// 2/8/22   [EDGcpfe/25052]
//
// C++-generating back end: Deduction guides
//
// The C++-generating back end previously mis-rendered this example in two places.
// First, the deduction guide member template was rendered twice (without a
// separating semicolon).  Second, the cast expression relying on class template
// argument deduction did not render the template arguments on the qualifier of
// the template name (i.e., "Keys::Vals" instead of "Keys<char, void>::Vals").
// Both problems are now fixed.
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
