//type:cp
//options_all:--il_display
//options:--c++03:--c++11:--c++20
//filter:grep -A4 'initializer_range' | edg-normalize-test-output --il

template <class T> struct S1{
  static const bool mem;
};
template <class T> const bool S1<T>::mem=true;

template <bool> struct S2 {typedef int type;};

class S3
{

 public:
  template< class T1,
    class T2= typename S2<S1<T1>::mem == 0>::type
    >
    S3(T1 param);
  template< class T1,
    class T2= typename S2<S1<T1>::mem>::type
    >
    S3(T1 param1, int param2);
  template< class T1,
    class T2= typename S2<S1<T1>::mem != 0>::type
    >
    void m_f(T1 param);
};

void f(void){
  S3 v1(1);
  // the behavior does not happen for all the following cases
  S3 v2(2, 3);
  v1.m_f(4);
  v2.m_f(5);
}
