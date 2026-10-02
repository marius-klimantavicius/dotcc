//type:fn
//options_all:--c++17 -tused

template <typename T> T var{};

template <int> struct A;

template <class, class = void> struct B;
template <class T>
struct B<T, typename A<noexcept(T(var<T>))>::type>;

template <class T>
struct C : B<T>
{};

struct D {
  D();
private:
  D(D &) {
    C<D> x;
  }
};
