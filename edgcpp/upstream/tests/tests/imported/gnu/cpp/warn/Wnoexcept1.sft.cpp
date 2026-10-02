//type: fp
//options: --c++11
# 0 "./warn/Wnoexcept1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Wnoexcept1.C"




# 1 "./warn/Wnoexcept1.h" 1

       
# 3 "./warn/Wnoexcept1.h" 3


# 4 "./warn/Wnoexcept1.h" 3
using size_t = decltype(sizeof(42));
inline void * operator new (size_t, void *p) noexcept { return p; }

template<typename _Up, typename... _Args>
void
construct1(_Up* __p, _Args... __args)
  noexcept(noexcept(::new((void *)__p) _Up(__args...)))
{ ::new((void *)__p) _Up(__args...); }

template<typename _Up, typename... _Args>
void
construct2(_Up* __p, _Args... __args)
  noexcept(noexcept(::new((void *)__p) _Up(__args...)))
{ ::new((void *)__p) _Up(__args...); }

class Automatic1 {
public:
  Automatic1(size_t bla) : Bla(bla) {};

private:
  size_t Bla;
};
# 6 "./warn/Wnoexcept1.C" 2







# 12 "./warn/Wnoexcept1.C"
class Automatic2 {
public:
  Automatic2(size_t bla) : Bla(bla) {};

private:
  size_t Bla;
};

union U
{
  unsigned char buf[sizeof(Automatic1)];
  Automatic1 a1;
  Automatic2 a2;
  U(): buf{} {}
  ~U() {}
};

int main() {
  U u;
  construct1(&u.a1, 42);
  construct2(&u.a2, 42);
}
