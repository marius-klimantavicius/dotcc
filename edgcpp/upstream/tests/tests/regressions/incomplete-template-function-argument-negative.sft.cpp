//type:fn
//options_all: --c++11
struct Incomplete;

template<typename T> void foo(T) { };

int main() {
  foo<Incomplete>(0);
}
