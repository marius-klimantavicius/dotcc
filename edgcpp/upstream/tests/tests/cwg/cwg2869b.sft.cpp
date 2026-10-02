//options_all:--c++23 -A
class C {
  void f() {
    [this] (this auto self) { return this->n; };
  }
  int n;
};
