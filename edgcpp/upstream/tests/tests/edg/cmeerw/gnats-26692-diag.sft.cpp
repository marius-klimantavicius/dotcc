//type:fn
//options:--c++20

const int ci1 = 1;
const int ci2 = 2;

extern const int eci1;
extern const int eci2;

const int &r1 = 1;
const int &r2 = 2;

extern const int &er1;
extern const int &er2;

enum class E { };

void f()
{
  // same types in error for next 4 lines
  E e1 = true ? ci1 : ci2;
  E e2 = true ? eci1 : eci2;
  E e3 = true ? r1 : r2;
  E e4 = true ? er1 : er2;

  // same types in error for next 4 lines
  *ci1;
  *eci1;
  *r1;
  *er1;
}
