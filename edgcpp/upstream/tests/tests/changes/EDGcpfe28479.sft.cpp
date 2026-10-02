//type:fp
//options_all:--gn 130201 --c11
//remark:[6.8] GNU/Clang C compatibility: Use of static const variables in constant expression
// 10/14/25 [EDGcpfe/28479]
//
// GNU/Clang C compatibility: Use of static const variables in constant expression
//
// Ordinarily, this is an error because in standard C the expression x.c is not
// constant.  However, recent versions of GCC and Clang do accept this code and
// the front end now emulates that behavior in its corresponding modes (when
// gnu_version >= 80000 or clang_version >= 170000, respectively).
typedef struct { int a; } A;
typedef struct { int c; } C;
void g() {
  static const C x = { 42 };
  static A y = { x.c };
}
