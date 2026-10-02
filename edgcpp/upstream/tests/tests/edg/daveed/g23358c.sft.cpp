//remark:Mixed static/non-static overload sets
//options:--c++14 --gnu=40902;fp


template<typename> struct is_int

{

  constexpr operator bool() { return false; }

};


template<> struct is_int<int>

{

  constexpr operator bool() { return true; }

};


template<bool> struct enable_if { };

template<> struct enable_if<true> { typedef void type; };


class blah {

public:

  template<typename T>

  typename enable_if<is_int<T>{}>::type

  func(const T&) {

  }

  template<typename T>

#ifndef OK

  static

#endif

  typename enable_if<!is_int<T>{}>::type

  func(const T&) {

  }

};


void func()

{

  blah x;

  x.func(5);

  //blah::func(5); // Not allowed

  blah::func(5.5);

}

