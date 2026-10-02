//options_all:-r -x -tused
//options: --strict;cp

template <class S, class T> int F(S,T);
template <class U, class V> struct S {
  static int smf(U, V);
};
template <class S, class T> int F(S s, T t)
{
  return ::S<S,T>::smf(s,t);
}


