struct VariablePair { int first; int second; };
int variable_cleanup_count;
void variable_cleanup(int *value) { variable_cleanup_count += *value; }

thread_local int standard_thread_value = 3;
constinit thread_local int constant_thread_value = 4;
inline int inline_variable_value = 5;
constexpr int constant_variable_value = 6;

#if defined(__GNUC__)
__thread int extension_thread_value = 7;
int variable_alias_target = 8;
extern int variable_alias __attribute__((alias("variable_alias_target")));
int weak_variable __attribute__((weak)) = 9;
int hidden_variable __attribute__((visibility("hidden"))) = 10;
int retained_variable __attribute__((used, section(".variable_test"))) = 11;
int tagged_variable __attribute__((abi_tag("variable_tag"))) = 12;
#endif

template<class T> constexpr bool variable_template_value = sizeof(T) > 1;
static_assert(variable_template_value<int>);

int variable_bindings(int input) {
  auto [first, second] = VariablePair{input, input + 1};
  int &first_reference = first;
  constexpr int local_constant = 2;
  int uninitialized;
  uninitialized = input;
#if defined(__GNUC__)
  int guarded __attribute__((cleanup(variable_cleanup))) = input + 2;
  return first_reference + second + guarded + uninitialized + local_constant;
#else
  return first_reference + second + uninitialized + local_constant;
#endif
}

int variable_lifetime_extension() {
  VariablePair const &extended = VariablePair{13, 14};
  return extended.first;
}

int tuple_binding_effects;
struct VariableTuple {
  int first;
  int second;
  template<unsigned Index> int &get() {
    ++tuple_binding_effects;
    if constexpr (Index == 0) return first;
    else return second;
  }
};

namespace std {
  template<class> struct tuple_size;
  template<unsigned, class> struct tuple_element;
  template<> struct tuple_size<VariableTuple> {
    static constexpr unsigned value = 2;
  };
  template<unsigned Index> struct tuple_element<Index, VariableTuple> {
    using type = int;
  };
}

int variable_tuple_bindings() {
  VariableTuple tuple{1, 2};
  auto &[first, second] = tuple;
  return first + second;
}
