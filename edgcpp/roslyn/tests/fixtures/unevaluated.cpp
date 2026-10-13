int unevaluated_effects;

int unevaluated_effect() {
  return ++unevaluated_effects;
}

int unevaluated_nothrow_effect() noexcept {
  return ++unevaluated_effects;
}

bool unevaluated_noexcept() {
  return noexcept(unevaluated_effect()) || noexcept(unevaluated_nothrow_effect());
}

template<class T>
bool unevaluated_dependent_noexcept(T &value) {
  return noexcept(value.effect());
}

#if defined(_MSC_VER)
void unevaluated_microsoft_assume() {
  __assume(unevaluated_effect() > 0);
}
#endif

#if defined(__clang__)
void unevaluated_builtin_assume() {
  __builtin_assume(unevaluated_effect() > 0);
}
#endif

#if defined(__GNUC__) || defined(__clang__)
int unevaluated_builtin_queries() {
  return __builtin_constant_p(unevaluated_effect())
       + __builtin_classify_type(unevaluated_effect());
}
#endif

int evaluated_effect_control() {
  return unevaluated_effect();
}
