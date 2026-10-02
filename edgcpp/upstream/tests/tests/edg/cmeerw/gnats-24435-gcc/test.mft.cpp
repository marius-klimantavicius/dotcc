//type:fp
//options_all:--c++14 --g++ --gnu_version=100100 --multi_trans_unit
//source_files:source2.C




[[ using ns: ]] static int v = 1; // warning: C++17-style "using" attribute prefix is nonstandard in this mode
[[ using ns: ]] static int w = 1; // already diagnosed

void init_statement1()
{
  int arr[3];

  if (++v; v > 0) // warning: C++17-style initializer is nonstandard in this mode
  { }

  if (++v; v > 0) // already diagnosed
  { }

  for (++w; int j : arr) // warning: C++20-style initializer statement in a range-based "for" statement is nonstandard in this mode
  { }

  for (++w; int j : arr) // already diagnosed
  { }
}

template<int ... I>
auto fold1()
{
  auto x = v + (0 + ... + I); // warning: fold expressions are nonstandard in this mode
  auto y = w + (0 + ... + I); // already diagnosed

  return x + y;
}

template auto fold1<1, 2, 3>();
