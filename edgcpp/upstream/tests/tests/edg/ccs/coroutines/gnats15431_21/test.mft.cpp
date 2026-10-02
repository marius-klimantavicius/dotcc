//type:fp
//options_all:--microsoft_version 1900 -d-mangled_names --set_flag coroutines -tused
//require:BACK_END_IS_CP_GEN_BE 1
//script:./cpgenbe_demangle
struct X {
 X operator co_await() { return X(); }
};
X operator co_await(X) { return X(); }
template <class T> auto f1(T t) -> decltype(t.operator co_await());
template <class T> auto f2(T t) -> decltype(operator co_await(t));
int main() {
  f1(X());
  f2(X());
}
