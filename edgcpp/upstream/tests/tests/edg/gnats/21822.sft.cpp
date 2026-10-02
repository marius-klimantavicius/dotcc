//type:fn
//options_all:--microsoft_v 1912 --ms_c++17
template <typename T> struct A;
 
template <>
struct A<void (*)()> {
              static const bool value = true;
};
 
template <typename T>
bool g(T t) {
              return A<T>::value;
}
 
void f() noexcept {}
 
int main() {
              return g(&f) ? 0 : 1; // Should compile in /std:c++14, fail with error: use of undefined type 'A<T>' in /std:c++17.
}
