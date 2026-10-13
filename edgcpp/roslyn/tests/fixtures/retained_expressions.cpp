int array_binding_copy(int (&values)[2]) {
  auto [left, right] = values;
  return left + right;
}

template<class... Types>
constexpr unsigned long expression_pack_size(Types... values) {
  return sizeof...(values);
}
unsigned long use_expression_pack_size() { return expression_pack_size(1, 2L); }

template<class T>
int dependent_braced_call(T& object) { return object.accept({1, 2}); }
struct BracedPair { int first, second; };
struct BracedAcceptor { int accept(BracedPair); };
int use_braced_call(BracedAcceptor& object) { return dependent_braced_call(object); }

template<class T> struct DeducibleBox { DeducibleBox(T); };
template<class T> using DeducibleAlias = DeducibleBox<T>;
DeducibleAlias deduced_alias(1);

template<int Index, class... Types>
auto select_pack_value(Types... values) { return values...[Index]; }
int use_pack_value() { return select_pack_value<1>(2, 3); }
