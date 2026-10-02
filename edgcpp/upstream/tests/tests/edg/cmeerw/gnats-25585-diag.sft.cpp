//type:fn
//options:--c++20:--ms_c++20 --microsoft_version=1927

namespace no_user_declared_deduction_guides_for_designators
{
  template<typename T>
  struct Y
  {
    int i;
    T t;
    int j;
  };

  template<typename T>
  Y(int, T) -> Y<T>;

  Y y1{ 1, 'b' };

  Y y2{ .t = 'a', .j = 2 };     // error: unable to deduce arguments
}
