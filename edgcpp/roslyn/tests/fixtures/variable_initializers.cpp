struct VariableInitializerClosures {
  inline static auto callback = [](int value) { return value + 1; };
  inline static auto nested = [](int value) {
    auto inner = [value](int offset) { return value + offset; };
    return inner(2);
  };
};
template<class T> struct VariableInitializerTemplate {
  inline static auto callback = [](T value) { return value + 3; };
};
int variable_initializers(int value) {
  return VariableInitializerClosures::callback(value)
    + VariableInitializerClosures::nested(value)
    + VariableInitializerTemplate<int>::callback(value);
}
