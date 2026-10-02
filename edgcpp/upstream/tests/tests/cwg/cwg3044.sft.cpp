//type:fp
//options:--c++26
//options_all:-A -tused

namespace std
{
  template<typename T, unsigned N>
  struct array
  {
    T arr[N];

    constexpr const int *begin() const { return arr; }
    constexpr const int *end() const { return arr + N; }
  };
}

consteval int f() {
  constexpr std::array<int, 3> arr {1, 2, 3};
  int result = 0;
  template for (constexpr int s : arr) { // OK, iterating expansion statement
    result += sizeof(char[s]);
  }
  return result;
}

static_assert(f() == 6);

//cwg: 3044
//title: Iterating expansion statements woes
//meeting: Kona 11/25
//edg_status: EDGcpfe/28538
