//type:cp
//options::--g++:--clang:--microsoft
//options_all:--c++11 -tused --multi_trans_unit
//source_files:gnats25352-2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

struct X {
  template<typename T> X(T);
};

void conv_func(X, X);

enum { E1 };
enum { E2 };

template<typename T>
inline void func() {
  conv_func(E1, E2);
}

auto fn = func<int>;
