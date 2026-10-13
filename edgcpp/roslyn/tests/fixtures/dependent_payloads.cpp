template<class T> struct NestedHolder {
  struct Nested { T value; };
  Nested nested;
};
NestedHolder<int> nested_holder;

template<int Width> struct DependentBits {
  _BitInt(Width) signed_value;
  unsigned _BitInt(Width + 1) unsigned_value;
};
DependentBits<9> dependent_bits;

template<class T> struct TyperefProxyHolder {
  using Result = decltype(T::make());
  typename Result::member value;
};
struct ProxyResult { using member = int; };
struct ProxyProducer { static ProxyResult make(); };
TyperefProxyHolder<ProxyProducer> typeref_proxy_holder;

template<class T> int convert_dependent(T value) { return value.operator int(); }
struct Convertible { operator int() const; };
int convert_actual(Convertible value) { return convert_dependent(value); }

template<class T> auto unknown_member_address() { return &T::field; }
template<class T> struct MemberPattern {
  int field;
  static auto own_member_address() { return &MemberPattern::field; }
};
auto concrete_member_address() { return MemberPattern<int>::own_member_address(); }

int nested_init_capture(int seed) {
  auto outer = [value = seed] {
    auto inner = [value] { return value; };
    return inner();
  };
  return outer();
}
int nested_variable_capture(int seed) {
  auto outer = [seed] {
    auto inner = [seed] { return seed; };
    return inner();
  };
  return outer();
}
