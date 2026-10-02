//type: fp
//options: --c++14
# 0 "./cpp1y/lambda-mangle-1-11.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp1y/lambda-mangle-1-11.C"





# 1 "./cpp1y/lambda-mangle-1.h" 1







template<typename T> class X;

template<typename T>
T &&forward (T &v)
{
  return static_cast<T &&> (v);
}

template<typename T>
void eat (T &v)
{
}

template<typename S, typename T>
  void eat (S &, T &v)
{
}

inline void Foo ()
{
  auto lam = [](auto &) { };
  auto lam_1 = [](int &, auto &) { };
  auto lam_2 = [](auto &, X<int> &) { };
  auto lam_3 = [](auto (*)[5]) { };

  forward (lam);
  forward (lam_1);
  forward (lam_2);
  forward (lam_3);

  eat (lam);
  eat (lam_1);
  eat (lam_2);
  eat (lam_3);


  auto lambda_1 = [](float *, float *) { };
  auto lambda_2 = [](auto *, auto *) { };
  auto lambda_3 = [](auto *, auto *) { };

  int *i;

  eat (i, lambda_1);
  eat (i, lambda_2);


  eat (lambda_2, lambda_3);
}

template<typename X> void Bar ()
{
  auto lambda_1 = [](X *, float *, float *) { };
  auto lambda_2 = [](X *, auto *, auto *) { };
  auto lambda_3 = [](X *, auto *...) {};

  int *i;

  eat (i, lambda_1);
  eat (i, lambda_2);
  eat (i, lambda_3);
}

void Baz ()
{
  Bar<short> ();
  Foo ();
}
# 7 "./cpp1y/lambda-mangle-1-11.C" 2
