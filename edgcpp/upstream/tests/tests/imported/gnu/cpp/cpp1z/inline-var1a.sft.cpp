//type: fp
//options: --c++17
# 0 "./cpp1z/inline-var1a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp1z/inline-var1a.C"



# 1 "./cpp1z/inline-var1.h" 1
inline int var1 = 4;
static inline int var7 = 9;
namespace N
{
  int inline var2;
  inline const int var6 = 8;
  static inline double var8 = 2.0;
  extern inline char var10;
}
struct S
{
  static constexpr int var3 = 5;
  static inline int var4 = 6;
  static constexpr int var5 = 7;
  static inline double var9 = 3.0;
  static constexpr inline int var11 = 11;
};
const int S::var3;
const int S::var3;
extern int foo (int);
extern int bar (int);
struct T { T () { t = foo (3); } T (int x) { t = foo (x); } int t; };
inline int var12 = foo (0);
int inline var13 = foo (1);
struct U
{
  static inline int var14 = foo (2);
  static inline T var15;
  static inline T var16 = 4;
  static int inline var17 = foo (5);
  static constexpr double var18 = 4.0;
};
template <typename T>
struct Y
{
  static constexpr T var24 = 6;
  static inline T var25 = 7;
  static inline int var26 = 8;
  static constexpr T var28 = 10;
};
template <typename T>
const T Y<T>::var24;
template <typename T>
const T Y<T>::var24;
template <typename T>
inline T var27 = 9;
# 5 "./cpp1z/inline-var1a.C" 2

static inline int var19 = bar (16);
static int inline var20 = bar (17);
inline int var21 = foo (6);
inline int var22 = foo (7);
extern inline int var23;
inline int var23 = foo (8);

int &alt1 = var1;
int &alt2 = N::var2;
const int &alt3 = S::var3;
int &alt4 = S::var4;
const int &alt5 = S::var5;
const int &alt6 = N::var6;
int &alt7 = var7;
double &alt8 = N::var8;
double &alt9 = S::var9;
const int &alt11 = S::var11;
int &alt12 = var12;
int &alt13 = var13;
int &alt14 = U::var14;
T &alt15 = U::var15;
T &alt16 = U::var16;
int &alt17 = U::var17;
const double &alt18 = U::var18;
int &alt19 = var19;
int &alt20 = var20;
int &alt21 = var21;
int &alt22 = var22;
int &alt23 = var23;
const int &alt24 = Y<int>::var24;
int &alt25 = Y<int>::var25;
int &alt26 = Y<int>::var26;
int &alt27 = var27<int>;
const int &alt28 = Y<int>::var28;
const char &alt24a = Y<char>::var24;
char &alt25a = Y<char>::var25;
int &alt26a = Y<char>::var26;
char &alt27a = var27<char>;
const char &alt28a = Y<char>::var28;
