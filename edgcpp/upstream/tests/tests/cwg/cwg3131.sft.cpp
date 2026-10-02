//type:fp
//options: -A --c++26

constexpr int arr[3] = {1, 2, 3};

struct span_like {
  const int *p;
  int n;

  constexpr const int *begin() const { return p; }
  constexpr const int *end() const { return p + n; }
};

consteval span_like foo() {
  return {arr, 3};
}

int main() {
  int r = 0;
  template for (constexpr auto m : foo())
    r += m;
  return r;
}

//cwg: 3131
//title: Value categories and types for the range in iterable expansion statements
//meeting: Croydon 3/26
