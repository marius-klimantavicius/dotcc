//options_all:-r -x -tused
//options: --microsoft -n;cp

/*
__declspec(selectany) int x1 = 1;
const __declspec(selectany) int x2=2;    // error
extern const __declspec(selectany) int x3=3;
extern const int x4;
const __declspec(selectany) int x4=4;
extern __declspec(selectany) int x5;     // error
void f() {
  __declspec(selectany) int y1 = 1;        // error
  const __declspec(selectany) int y2=2;    // error
  extern __declspec(selectany) int y3;     // error
}
void g(__declspec(selectany) int) { }           // error
void h() {
  try { } catch(__declspec(selectany) int) { }  // error
}
void __declspec(selectany) ff();
struct __declspec(selectany) A;
*/
struct B {
//  int __declspec(selectany) i;
//  static int __declspec(selectany) j;
  static int k;
};
int __declspec(selectany) B::k = 0;

