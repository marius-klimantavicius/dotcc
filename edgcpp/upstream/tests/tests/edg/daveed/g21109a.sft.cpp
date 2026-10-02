//remark:ADL and SFINAE
//options:--c++17;fp:--c++17 --microsoft_v=1920;fp

template<class> void get();

template<typename T>
void adl_get(T t) noexcept(noexcept(void(get<0>(t))));

namespace ns {
template<typename T> struct A {};
template <int I, typename T> void get(A<T>&);
}

int main() {
  adl_get(ns::A<int>{});
}
