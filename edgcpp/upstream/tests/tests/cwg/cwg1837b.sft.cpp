//type:fn
//options_all:--c++20 -tused -A
  template <typename T>
  struct Fish { static const bool value = true; };

  struct Other {
    int p();
    auto q() -> decltype(p()) *;
  };

  class Outer {
    // The following declares a member function of class Other.
    int g();
    int f() {
     extern void f(decltype(this->g()) *);
     struct Inner {
       // The following are all within the declaration of Outer::f().
       static_assert(Fish<decltype(this->g())>::value, ""); /*error */
       enum { X = Fish<decltype(this->f())>::value };	/* error */
       struct Inner2 : Fish<decltype(this->g())> { };	/* error */
       friend void f(decltype(this->g()) *);	/* error */
       friend auto Other::q() -> decltype(this->p()) *;	/* error */
     };
     return 0;
    }
  };
