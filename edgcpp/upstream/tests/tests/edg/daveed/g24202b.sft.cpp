//remark:Partial specialization and SFINAE
//options:--c++20;fn:--c++20 --microsoft_v=1928;fn:--clang_v=120000;fn


  template<typename T, typename U> struct S {};
  template<typename T> struct S<T, T> {};
  template<typename T, typename U> struct S<T*, U*> {};
  template<typename ... Ts> using V = void;
  template<typename T, typename U = void> struct X {};
  template<typename T> struct X<T, V<typename S<T, T>::type>>;
  X<int*> xpi;  // Now accepted in Clang and Microsoft C++ modes.
  S<int*, int*>::undefined u = "hello";
                // Still an error.
