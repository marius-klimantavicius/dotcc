  // header "S.h"
  
  template<class T>
  struct S {
    S(const T*);
  };
  template<class T>
  S(T*) -> S<T>;
