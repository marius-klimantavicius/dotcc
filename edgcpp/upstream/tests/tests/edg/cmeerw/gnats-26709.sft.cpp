//type:fn
//options:--c++17 --defer_parse_function_templates -tused:--c++20 --defer_parse_function_templates -tused:--ms_c++17 --microsoft_version 1942 --no_ms_permissive --defer_parse_function_templates -tused:--ms_c++17 --microsoft_version 1942 --ms_permissive --defer_parse_function_templates -tused;fp

namespace minimal
{
  template<typename T>
  struct C {
    C() { f(); }
    void f() {
      if constexpr (false) {
        undefined_id;  // Previously skipped, now an error.
      }
    }
  };
  C<int> c;
}
