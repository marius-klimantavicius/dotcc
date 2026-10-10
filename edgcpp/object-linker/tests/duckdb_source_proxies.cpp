template<class T> T &&declval();
template<class P> auto address(P const &p) -> decltype(declval<P const &>().operator->()) { return p.operator->(); }
struct Pointer { int *operator->() const; };
template<class T> concept Selected = requires { address(Pointer{}); };
template<Selected T> struct Retain { T value = {}; };
// The cast destination is lexical type syntax; its alias argument retains the
// requires-parameter binder even when proxy result annotations see it first.
template<class T> struct Identity { using type = T; };
template<class T> using Decay = typename Identity<T>::type;
template<class T> concept Any = true;
template<class T> concept MemberData = requires(T&& t) {
  { static_cast<Decay<decltype(t.data())>>(t.data()) } -> Any;
};
template<MemberData T> struct RetainCast { T value = {}; };
int main() { return 0; }
