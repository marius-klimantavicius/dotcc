//type:fp
//options: -A --c++26

struct array_like {
  int d[3];
  constexpr int *begin() { return d; }
  constexpr int *end() { return d + 3; }
};

int main() {
  array_like a{{1, 2, 3}};
  int s = 0;
  template for (auto x : a) {
    s += x;
  }
  return s;
}

//cwg: 3140
//title: Allowing expansion over non-constant std::array
//meeting: Croydon 3/26
