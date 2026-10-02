//type:fp
//options: -A --c++26

template <typename, typename>
constexpr bool same = false;

template <typename T>
constexpr bool same<T, T> = true;

struct V {};
struct L {};
struct R {};

struct triple {
  V a;
  L &b;
  R &&c;
};

L gl;
R gr;

triple make() {
  return {V{}, gl, static_cast<R &&>(gr)};
}

int main() {
  // make() is an rvalue, so in each iteration the binding must preserve the
  // value category of the corresponding element: V&& (value member),
  // L& (lvalue-reference member), R&& (rvalue-reference member).
  template for (auto &&elem : make()) {
    using E = decltype(elem);
    static_assert(same<E, V &&> || same<E, L &> || same<E, R &&>);
  }
}

//cwg: 3149
//title: Rvalues in destructuring expansion statements
//meeting: Croydon 3/26
