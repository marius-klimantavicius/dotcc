//remark:Generated move operations
//options:--c++11;fp:--c++11 --gnu_version=40700;fn

struct B {
  B(B&&);
  B& operator=(B&&);
};
struct D: virtual B {
};

void f(D &&d) {
  d = static_cast<D&&>(d);
}

