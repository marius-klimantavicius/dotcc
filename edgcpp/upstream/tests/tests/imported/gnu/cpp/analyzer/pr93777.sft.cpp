//type: fp
//options: 
# 0 "./analyzer/pr93777.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr93777.C"
# 1 "./analyzer/../../g++.old-deja/g++.pt/spec36.C" 1







extern "C" int puts (char const *);

template <typename T> int Foo (T *) {puts (__PRETTY_FUNCTION__); return 1;}
template <typename T> int Foo (T &) {puts (__PRETTY_FUNCTION__); return 2;}
template <typename T> int Foo (T const &) {puts (__PRETTY_FUNCTION__); return 3;}

template <typename T> int Bar (T const *const &) {puts (__PRETTY_FUNCTION__); return 4;}
template <typename T> int Bar (T *const &) {puts (__PRETTY_FUNCTION__); return 5;}
template <typename T> int Bar (T *) {puts (__PRETTY_FUNCTION__); return 6;}

template <typename T> int Quux (T *const &) {puts (__PRETTY_FUNCTION__); return 7;}
template <typename T> int Quux (T const &) {puts (__PRETTY_FUNCTION__); return 8;}


int Baz (int const *ptr, int *ptr2)
{
  if (Foo (ptr) != 1)
    return 1;
  if (Foo (ptr2) != 1)
    return 2;
  if (Foo (*ptr) != 3)
    return 3;
  if (Foo (*ptr2) != 2)
    return 4;

  if (Bar (ptr) != 4)
    return 5;

  if (Quux (ptr) != 7)
    return 5;
  if (Quux (ptr2) != 7)
    return 6;

  return 0;
}

int main ()
{
  return Baz (0, 0);
}
# 2 "./analyzer/pr93777.C" 2
