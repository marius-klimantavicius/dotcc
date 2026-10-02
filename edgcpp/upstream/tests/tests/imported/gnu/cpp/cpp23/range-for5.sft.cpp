//type: rp
//options: --c++11
# 0 "./cpp23/range-for5.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/range-for5.C"







# 1 "./cpp23/range-for1.C" 1
# 15 "./cpp23/range-for1.C"
static_assert (200907L >= 200907L
        && 200907L < 201603L, "");



extern "C" void abort ();
void check (bool);

struct S
{
  S () { ++s; }
  S (const S &) { ++s; }
  ~S () { check (true); --s; }
  static int s;
};

int S::s = -1;
S sv;

struct T
{
  T (const S &, const S &) { ++t; }
  T (const T &) { ++t; }
  ~T () { check (false); --t; }
  static int t;
};

int T::t = -1;
T tv (sv, sv);
int a[4];
int c;

void
check (bool is_s)
{
  if (c)
    {
      if (is_s)
 {
   if (T::t != (c == 1))
     abort ();
 }
      else
 {
   if (S::s != (c == 1 ? 0 : 2))
     abort ();
 }
    }
}

template <typename T>
int *
begin (const T &)
{
  return &a[0];
}

template <typename T>
int *
end (const T &)
{
  return &a[4];
}

const S &
foo (const S &)
{
  return sv;
}

const T &
foo (const T &)
{
  return tv;
}

void
bar ()
{
  if (S::s != 0)
    abort ();
  for (auto x : S ())
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : foo (S ()))
    {
      if (S::s != 0)
 abort ();
    }
  if (S::s != 0)
    abort ();
  if (T::t != 0)
    abort ();
  c = 1 + 0;
  for (auto x : T (S (), S ()))
    {
      if (S::s != 2 * 0 || T::t != 1)
 abort ();
    }
  if (S::s != 0 || T::t != 0)
    abort ();
  c = 2;
  for (auto x : foo (T (S (), S ())))
    {
      if (S::s != 2 * 0
   || T::t != 0)
 abort ();
    }
  if (S::s != 0 || T::t != 0)
    abort ();
  c = 0;
}

template <int N>
void
baz ()
{
  if (S::s != 0)
    abort ();
  for (auto x : S ())
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : foo (S ()))
    {
      if (S::s != 0)
 abort ();
    }
  if (S::s != 0)
    abort ();
  if (T::t != 0)
    abort ();
  c = 1 + 0;
  for (auto x : T (S (), S ()))
    {
      if (S::s != 2 * 0 || T::t != 1)
 abort ();
    }
  if (S::s != 0 || T::t != 0)
    abort ();
  c = 2;
  for (auto x : foo (T (S (), S ())))
    {
      if (S::s != 2 * 0
   || T::t != 0)
 abort ();
    }
  if (S::s != 0 || T::t != 0)
    abort ();
  c = 0;
}

template <typename S, typename T>
void
qux ()
{
  if (S::s != 0)
    abort ();
  for (auto x : S ())
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : foo (S ()))
    {
      if (S::s != 0)
 abort ();
    }
  if (S::s != 0)
    abort ();
  if (T::t != 0)
    abort ();
  c = 1 + 0;
  for (auto x : T (S (), S ()))
    {
      if (S::s != 2 * 0 || T::t != 1)
 abort ();
    }
  if (S::s != 0 || T::t != 0)
    abort ();
  c = 2;
  for (auto x : foo (T (S (), S ())))
    {
      if (S::s != 2 * 0
   || T::t != 0)
 abort ();
    }
  if (S::s != 0 || T::t != 0)
    abort ();
  c = 0;
}

int
main ()
{
  bar ();
  baz <0> ();
  qux <S, T> ();
}
# 9 "./cpp23/range-for5.C" 2
