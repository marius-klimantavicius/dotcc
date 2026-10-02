//options_all:--microsoft_version 1920 --microsoft_bugs
template <bool B, class T = void>
struct enable_if {};
 
template <class T>
struct enable_if<true, T> {
    typedef T type;
};
 
template <typename T>
struct X {
              template <typename U>
              static constexpr T Value = 3;
};
 
template <typename T1, typename T2>
typename enable_if<X<T1>::Value<T2> == 3, int>::type f() {
              return 4;
}
 
int main() {
              f<int, char>();
}
