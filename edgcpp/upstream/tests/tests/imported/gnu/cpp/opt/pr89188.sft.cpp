//type: fp
//options: --c++11
# 0 "./opt/pr89188.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/pr89188.C"




# 1 "./opt/../torture/pr88861.C" 1


struct Ax {
  int n, a[];
};




int i = 12345678;

int main() {
  static Ax s{456, i};
  ((s.a[0]) ? (void)0 : (void)0);
}
# 6 "./opt/pr89188.C" 2
