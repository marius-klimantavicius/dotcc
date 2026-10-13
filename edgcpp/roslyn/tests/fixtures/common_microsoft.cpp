namespace CommonProxyNamespace {}
template<class T> auto common_proxy_return(T) -> decltype(CommonProxyNamespace::not_declared);
struct CommonMicrosoftBase { int base_member; };
template<class T> struct CommonUnknownBase : T {
  int unqualified() { return unknown_member; }
  int qualified() { return T::unknown_member; }
};
int __identifier(class) = 23;
int common_microsoft(int value) {
  __try {
    if (value) __leave;
    value += __identifier(class);
  } __finally {
    value += 1;
  }
  return value;
}
