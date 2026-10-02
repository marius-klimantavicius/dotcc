//type:cp
//options_all:--c++20 --g++

namespace std {
  struct source_location {
    struct __impl {
      const char *_M_file_name;
      const char *_M_function_name;
      unsigned int _M_line, _M_column;
    };
    const __impl *__ptr;
    constexpr source_location () : __ptr (nullptr) {}
    static consteval source_location
    current (const void *__p = __builtin_source_location ()) {
      source_location __ret;
      __ret.__ptr = static_cast <const __impl *> (__p);
      return __ret;
    }
    constexpr const char *file_name () const {
      return __ptr ? __ptr->_M_file_name : "";
    }
    constexpr const char *function_name () const {
      return __ptr ? __ptr->_M_function_name : "";
    }
    constexpr unsigned line () const {
      return __ptr ? __ptr->_M_line : 0;
    }
    constexpr unsigned column () const {
      return __ptr ? __ptr->_M_column : 0;
    }
  };
}

using namespace std;

struct S {
  int j = source_location::current().line();
};

static_assert(S{}.j == 40);
