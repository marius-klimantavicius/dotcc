//remark:UDC and overload resolution error recovery
//options:--c++17 --gnu=90400;fn


struct S {
  S(float);
  S(short);
};
struct T {
  operator float() const;
  operator double() const;
};
void foo() {
  T s;
  static_cast<S>(s);
}

