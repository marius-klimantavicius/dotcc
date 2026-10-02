//type:fp
//options_all:--c++14 --clang --clang_version=100100 --multi_trans_unit
//source_files:source2.C




[[ using ns: ]] static int v = 1; // warning: C++17-style "using" attribute prefix is nonstandard in this mode
[[ using ns: ]] static int w = 1; // already diagnosed

auto foo1()
{
  return v + w;
}

namespace ns::nested // warning: C++17-style nested namespaces are nonstandard in this mode
{ }

namespace ns::nested // already diagnosed
{ }

namespace ns::inline inested // warning: C++20-style nested inline namespaces are nonstandard in this mode
{ }

namespace ns::inline inested // already diagnosed
{ }
