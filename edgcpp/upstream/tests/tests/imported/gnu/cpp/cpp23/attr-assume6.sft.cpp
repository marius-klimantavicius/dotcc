//type: rp
//options: --c++11
# 0 "./cpp23/attr-assume6.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/attr-assume6.C"




# 1 "./cpp23/attr-assume3.C" 1



namespace std
{
  constexpr bool
  isfinite (float x)
  { return __builtin_isfinite (x); }

  constexpr bool
  isfinite (double x)
  { return __builtin_isfinite (x); }

  constexpr bool
  isfinite (long double x)
  { return __builtin_isfinite (x); }

  constexpr float
  sqrt (float x)
  { return __builtin_sqrtf (x); }

  constexpr double
  sqrt (double x)
  { return __builtin_sqrt (x); }

  constexpr long double
  sqrt (long double x)
  { return __builtin_sqrtl (x); }

  extern "C" void
  abort ();
}

constexpr int
f1 (int i)
{




  return sizeof (int);

}

void
f2 ()
{
  static_assert (f1 (0) >= sizeof (int), "");
}

int
f3 (int i)
{
  [[gnu::assume (i == 42)]];
  return i;
}

int
f4 (int i)
{
  __attribute__ ((assume (++i == 44)));
  return i;
}

int a;
int *volatile c;

bool
f5 ()
{
  ++a;
  return true;
}

constexpr int
f6 ()
{



  return 1;
}

template <int ...args>
bool
f7 ()
{




  return true;

}

bool
f8 (double x)
{
  [[gnu::assume (std::isfinite (x) && x >= 0.0)]];
  return std::isfinite (std::sqrt (x));
}

double
f9 (double x)
{
  __attribute__((assume (std::isfinite (std::sqrt (x)))));
  return std::sqrt (x);
}

template <typename T, T N>
T
f10 (T x)
{
  [[__assume__ (x == N)]];
  return x;
}

int
f11 (int x)
{
  [[gnu::assume (x == 93 ? true : throw 1)]];
  return x;
}

constexpr int
f12 (int x)
{



  return x;
}

static_assert (f12 (42) == 42, "");

struct S
{
  operator bool () { return true; }
};

int
f13 ()
{
  S s;
  [[__gnu__::__assume__ (s)]];
  return 0;
}

template <typename T>
int
f14 ()
{
  T t;
  __attribute__((assume (t)));
  return 0;
}

int
main ()
{
  int b = 42;
  double d = 42.0, e = 43.0;
  c = &b;
  [[__assume__ (f5 ())]];
  if (a)
    std::abort ();
  [[gnu::assume (++b == 43)]];
  if (b != 42 || *c != 42)
    std::abort ();
  static_assert (f6 () == 1, "");
  if (f6 () != 1)
    std::abort ();
  if (a)
    std::abort ();
  if (!f7 <0> () || !f7 <1, 2, 3, 4> ())
    std::abort ();
  __attribute__((assume (d < e)));
  if (f10 <int, 45> (45) != 45
      || f10 <long long, 128LL> (128LL) != 128LL



      || false)
    std::abort ();
  int i = 90, j = 91, k = 92;
  [[__assume__ (i == 90), gnu::assume (j <= 91)]]



  [[gnu::assume (k >= 92)]]

  ;
  __attribute__((__assume__ (i == 90), assume (j <= 91))) __attribute__((assume (k >= 92)));
  if (f11 (93) != 93)
    std::abort ();
  if (f14 <S> () != 0)
    std::abort ();
}
# 6 "./cpp23/attr-assume6.C" 2
