//type:cp
//options:--c++20:--c++20 -DNEG;fn:--c++17;fn

void f(int(&)[]);
void g(int(&)[]);
void g();

#if NEG
void h(int(&&)[]);
void h();
#endif

void test() {
  int arr[1];
  f(arr);
  g(arr);
#if NEG
  h(arr);
#endif
}
