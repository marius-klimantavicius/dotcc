struct ConstantEmpty {};
struct ConstantBase { int base; };
struct ConstantAggregate : ConstantBase {
  [[no_unique_address]] ConstantEmpty empty;
  int elements[4];
};
constexpr ConstantAggregate constant_aggregate{{3}, {}, {7, 9}};
constexpr ConstantAggregate constant_parenthesized({5}, {}, {11, 13});
constexpr const int *constant_element_address = &constant_aggregate.elements[1];
constexpr const int *constant_one_past_address = constant_aggregate.elements + 4;
constexpr const ConstantBase *constant_base_address = &constant_aggregate;
constexpr int constant_value() { return 23; }
constexpr int constant_evaluated = constant_value();
constexpr unsigned constant_mask = 0xffU & 0x3fU;
constexpr int constant_zero = 0;
constexpr auto constant_null = nullptr;
constexpr int *constant_cast_zero = static_cast<int *>(nullptr);

const char constant_ordinary[] = "same";
const char constant_raw[] = R"(same)";
const char8_t constant_utf8[] = u8"same";
const char16_t constant_utf16[] = u"same";
const char32_t constant_utf32[] = U"same";
const wchar_t constant_wide[] = L"same";
const char constant_embedded_null[] = "a\0b";
inline const char *constant_inline_string() { return "same"; }
const char *constant_function_name() { return __FUNCTION__; }
template<class T> const char *constant_template_function_name() { return __PRETTY_FUNCTION__; }
const char *constant_template_function_name_use() { return constant_template_function_name<int>(); }

struct ConstantPolymorphic {
  int value;
  constexpr ConstantPolymorphic() : value(47) {}
  virtual constexpr int read() const { return value; }
};
constexpr ConstantPolymorphic constant_polymorphic;
int constant_runtime_value();
struct ConstantDynamicAggregate { int first; int second; };
ConstantDynamicAggregate constant_dynamic = {constant_runtime_value(), 41};

template<ConstantAggregate Value> constexpr int constant_object_parameter() {
  return Value.elements[0];
}
int constant_object_use() { return constant_object_parameter<constant_aggregate>(); }
template<int N> int constant_dependent_expression() { return N + 1; }
template<class T> int constant_dependent_sizeof(T value) { return sizeof(value); }
template<class T> struct ConstantGeneric { T value; int tail; };
template<class T> ConstantGeneric<T> constant_generic_value = {T(), 31};
template<class T> using ConstantArrayAlias = T[2];
template<class T> struct ConstantAliasUse { ConstantArrayAlias<T> elements; };
ConstantAliasUse<int> constant_alias_use{};
template<class T> ConstantGeneric<T> constant_generic_initializer(T value) {
  return {value, 31};
}

#if defined(__GNUC__) && !defined(_MSC_VER)
int constant_statement_expression() { return ({ constexpr int value = 37; value; }); }
constexpr void *constant_gnu_null = __null;
#endif
