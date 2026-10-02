//type:cp
//options::--g++
//options_all:--c++11 -tused

namespace std {
  inline
  namespace __1 {
        
    template <class _Tp>
    void 
    swap(_Tp& __x, _Tp& __y) 
    {
    }
  }
}

class X {};

namespace std {
    
  template<> inline void
  swap(X& lhs, X& rhs)
  {
  }
}

void f() {
  X a, b;
  std::swap(a, b);
}
