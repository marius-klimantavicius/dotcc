//remark: constexpr integer @= float
//options:--c++14;fp

constexpr int f(int i)
{
  return i -= 0.0f;
}

constexpr int r = f(0);
