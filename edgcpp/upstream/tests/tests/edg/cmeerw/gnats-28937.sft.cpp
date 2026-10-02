//type:fp
//options:--c++11:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  using uint1 = unsigned int;
  namespace ns {
    enum E : uint1 {};
  }
  namespace ns {
    using uint2 = unsigned int;
    enum E : ns::uint2;
  }
}
