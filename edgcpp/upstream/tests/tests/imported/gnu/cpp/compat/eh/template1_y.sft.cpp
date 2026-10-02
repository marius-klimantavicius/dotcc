//type: fp
//options: 
# 0 "./compat/eh/template1_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/template1_y.C"
# 1 "./compat/eh/template1.h" 1
class A {};

template <class T>
struct B
{
  typedef A E;
};

template <class T>
struct C
{
  typedef B<T> D;
  typedef typename D::E E;
  void f()



  ;
};
# 2 "./compat/eh/template1_y.C" 2

template<class T> void C<T>::f (void)



{
  throw E();
}

template class C<int>;
