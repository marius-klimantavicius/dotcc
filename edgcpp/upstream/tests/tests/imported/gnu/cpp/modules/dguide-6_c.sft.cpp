//type: fp
//options:  --c++20 --modules
# 0 "./modules/dguide-6_c.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/dguide-6_c.C"


# 1 "./modules/dguide-6.h" 1
template <typename T> struct S {
  S(int);
  S(int, int);
};
# 4 "./modules/dguide-6_c.C" 2
import M;

int main() {
  S a(1);
  S<int> a_copy = a;

  S b(2, 3);
  S<double> b_copy = b;
}
