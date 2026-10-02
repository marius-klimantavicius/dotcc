//type:fn
//options_all:--microsoft_v 1920 --ms_c++17
template <typename T>
int f() {
              T::Type a;
              if constexpr (T::val) {
                             return 1;
              } else {
                             return 2;
              }
}
 
struct X {
              using Type = X;
              constexpr static int val = 1;
};
 
int main() {
              return f<X>();
}
