//remark:Substitution of defaulted friend comparison
//options:--c++20;fp

  template<typename T> requires requires(const T& x) { x == x; }
    void g();
  template<typename> struct X {
    friend bool operator==(X const&, X const&) = default;
  };
  int main() {
    g<X<int>>();
  }
