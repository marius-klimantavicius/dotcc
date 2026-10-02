//type:fp
//options::-DNEG;fn
//options_all:--c++11

template<class X, class Y> X f(Y);

template<class X, class Y, class ... Z> X g(Y);

void h() {
  int i = f<int>(5.6);   // Y is deduced to be double
#ifdef NEG
  int j = f(5.6);        // ill-formed: X cannot be deduced
#endif /* NEG */
  f<void>(f<int, bool>); // Y for outer f deduced to be int (*)(bool)
#ifdef NEG
  f<void>(f<int>);       // ill-formed: f<int> does not denote a single function template specialization
#endif /* NEG */
  int k = g<int>(5.6);   // Y is deduced to be double, Z is deduced to an empty sequence
  f<void>(g<int, bool>); // Y for outer f is deduced to be int (*)(bool),
                         // Z is deduced to an empty sequence
}
