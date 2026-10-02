//type:fn
//options_all: --c++11

template<typename T, typename U> struct TemplatedIncomplete;
template<typename T, typename U> struct TemplatedIncomplete<T*, U>;
template<typename T, typename U> struct TemplatedIncomplete<T, U*>;

template<typename T> void foo(T) { };

int main() {
  foo<TemplatedIncomplete<int*, int*>>(0);
}
