//remark:Bit fields and aligned typedefs
//options:--gcc;rp

int printf(char const*, ...);
typedef int PI __attribute__((aligned(1)));
struct S {
  char a;
  PI x : 25;
} s[2];

char* byte(int n) { return (char*)&s[n]; }
int main() {
  printf("%d\n", (int)(byte(1)-byte(0)));
  return (int)(byte(1)-byte(0)) != 5;
}
