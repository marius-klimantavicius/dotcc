//type:fp
//options_all:--microsoft_version 1914 --c++17
//remark:[5.1] Microsoft compatibility: Tuple-like structured bindings
// 11/27/18 [EDGcpfe/20191]
//
// Microsoft compatibility: Tuple-like structured bindings
//
// The C++17 standard specifies that a structured binding should use a tuple-like
// mechanism for value extraction if std::tuple_size<E>, where E is the type to be
// decomposed, is a complete type.  However, the Microsoft compiler further
// requires that std::tuple_size<E>::value actually be a constant integer value
// before committing to the tuple-like interpretation.
//
// Here, the type to be decomposed (in (1)) is "S const" and tuple_size<S const>
// is a complete type: The standard therefore requires the decomposition to use
// a tuple-like mechanism to decompose s in std::tuple_size<S const>::value
// components.  Since std::tuple_size<S const>::value isn't an integral constant
// expression, however, the program is ill-formed.  In Microsoft mode, however,
// the front end now falls back to decomposing s by its data members (ignoring the
// tuple-like mechanism).
namespace std {
  using size_t = decltype(sizeof(42));
  template<typename> struct tuple_size;
  template<typename, typename = void> struct _Base_tuple_size {};
  template<typename _T>
  struct _Base_tuple_size<_T, decltype(tuple_size<_T>::value)> {
    static constexpr size_t value = tuple_size<_T>::value;
  };
  template<typename _T>
  struct tuple_size<_T const>: _Base_tuple_size<_T> {};
}
struct S {
  int i, value;
} s;
int main() {
  const auto &[x, y] = s;  // (1)
};
