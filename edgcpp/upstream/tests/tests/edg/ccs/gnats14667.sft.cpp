//type:rp
//options:--c++11;fn:--c++14:--c++20

extern "C" int printf(const char*,...);

void f(const int (&)[3]) { printf("3\n"); }
void f(const int (&)[4]) { printf("4\n"); }

int main() {
  f({1, 2, 3});
}
