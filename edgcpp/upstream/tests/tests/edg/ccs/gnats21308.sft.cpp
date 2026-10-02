//type:cp
//options::--microsoft:--g++ -DUNSIGNED:--clang -DUNSIGNED
//options_all:--c++11

template <typename T1, typename T2>
struct is_same;

template<typename T>
struct is_same<T,T> { };

enum E {  };
enum F { a };
typedef __underlying_type(E) e_type;
typedef __underlying_type(F) f_type;

void f() {
#if UNSIGNED
  is_same<e_type, unsigned int>();
  is_same<f_type, unsigned int>();
#else
  is_same<e_type, int>();
  is_same<f_type, int>();
#endif
}
