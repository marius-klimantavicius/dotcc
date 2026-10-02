//type: rp
//options: --c++11
# 0 "./cpp23/range-for4.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp23/range-for4.C"






# 1 "./cpp23/range-for2.C" 1







extern "C" void abort ();

int a[4];

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

struct S
{
  S () { ++s; }
  S (const S &) { ++s; }
  ~S () { --s; }
  static int s;
};

int S::s;

template <typename T>
struct U
{
  T t;
  U () { ++u; }
  U (const U &) { ++u; }
  ~U () { --u; }
  const int *begin () const { return ::begin (t); }
  const int *end () const { return ::end (t); }
  U &foo () { return *this; }
  U bar () { return U (); }
  static int u;
};

template <typename T>
int U<T>::u;

template <typename T>
U<T>
foo ()
{
  return U<T> {};
}

template <typename T>
T
fred (const T &, const T & = T{}, const T & = T{})
{
  return T {};
}

void
bar ()
{
  int a[10] = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
  if (S::s != 0)
    abort ();
  for (auto x : S (), a)
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : (void) S (), a)
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : static_cast<void> (S ()), a)
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : foo <S> ().foo ().bar ().foo ().bar ().foo ().bar ())
    {
      if (S::s != 1 + 3 * 1)
 abort ();
      if (U<S>::u != S::s)
 abort ();
    }
  if (S::s != 0 || U<S>::u != 0)
    abort ();
  for (auto x : fred (S {}))
    {
      if (S::s != 1 + 3 * 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : fred (fred (S {}, S {})))
    {
      if (S::s != 1 + 6 * 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
}

template <int N>
void
baz ()
{
  int a[10] = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
  if (S::s != 0)
    abort ();
  for (auto x : S (), a)
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : (void) S (), a)
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : static_cast<void> (S ()), a)
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : foo <S> ().foo ().bar ().foo ().bar ().foo ().bar ())
    {
      if (S::s != 1 + 3 * 1)
 abort ();
      if (U<S>::u != S::s)
 abort ();
    }
  if (S::s != 0 || U<S>::u != 0)
    abort ();
  for (auto x : fred (S {}))
    {
      if (S::s != 1 + 3 * 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : fred (fred (S {}, S {})))
    {
      if (S::s != 1 + 6 * 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
}

template <typename S>
void
qux ()
{
  int a[10] = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
  if (S::s != 0)
    abort ();
  for (auto x : S (), a)
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : (void) S (), a)
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : static_cast<void> (S ()), a)
    {
      if (S::s != 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : foo <S> ().foo ().bar ().foo ().bar ().foo ().bar ())
    {
      if (S::s != 1 + 3 * 1)
 abort ();
      if (U<S>::u != S::s)
 abort ();
    }
  if (S::s != 0 || U<S>::u != 0)
    abort ();
  for (auto x : fred (S {}))
    {
      if (S::s != 1 + 3 * 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
  for (auto x : fred (fred (S {}, S {})))
    {
      if (S::s != 1 + 6 * 1)
 abort ();
    }
  if (S::s != 0)
    abort ();
}

int
main ()
{
  bar ();
  baz <0> ();
  qux <S> ();
}
# 8 "./cpp23/range-for4.C" 2
