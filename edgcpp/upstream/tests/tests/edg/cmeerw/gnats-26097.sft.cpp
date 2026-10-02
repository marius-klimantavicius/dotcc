//type:fp
//options:--c++11 --microsoft:--ms_c++17:--ms_c++20 --microsoft_version 1934:--c++11 --clang --ms_extensions;fn:--c++17 --clang --ms_extensions;fn:--c++11 --clang --ms_compatibility;fn:--c++17 --clang --ms_compatibility;fn

#ifdef __EDG__
namespace std
{
#pragma define_type_info
#endif
  class type_info
  {
  public:
    virtual ~type_info();
    const char* name() const;
  };
#ifdef __EDG__
}
#else
namespace std
{
  using ::type_info;
}
#endif

namespace minimal
{
  struct __declspec(uuid("00000000-0000-0000-0000-000000000000")) C;
  template<const _GUID &> struct A;
  A<__uuidof(C)> *a;
  template<const std::type_info &> struct B;
  B<typeid(int)> *b;            // Only accepted by MSVC, but not by Clang
}
