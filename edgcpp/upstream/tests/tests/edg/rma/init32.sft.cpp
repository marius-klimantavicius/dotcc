//options_all:-r -x -tused
//options: --strict;cp

char x[3] = "ab"; 
char y[2][3] = { "ab", "ab" };
char z[3]("ab");
void f() {
  char x[3] = "ab";
  char y[2][3] = { "ab", "ab" };
  char z[3]("ab");
}
void g() {
  static char x[3] = "ab";
  static char y[2][3] = { "ab", "ab" };
  static char z[3]("ab");
}
// Bug EDGqa00446
struct A {
  char t[3];
  A() : t("ab") {} 
} a;

