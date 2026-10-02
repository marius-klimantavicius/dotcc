//type:fn
//options_all:--c++20

namespace std {
  namespace detail {
    namespace v1 {
      template<typename T>
      struct vector_impl {
      };

      template<typename T>
      struct basic_string {
      };
    }
  }

  template<typename T>
  using vector = detail::v1::vector_impl<T>;

  using string = detail::v1::basic_string<char>;

  inline namespace v1 {
    template<typename T>
    using inline_ns_vector = detail::v1::vector_impl<T>;

    using inline_ns_string = detail::v1::basic_string<char>;
  }
}

namespace not_std {
  namespace detail {
    namespace v1 {
      template<typename T>
      struct vector_impl {
      };

      template<typename T>
      struct basic_string {
      };
    }
  }

  template<typename T>
  using vector = detail::v1::vector_impl<T>;

  using string = detail::v1::basic_string<char>;

  inline namespace v1 {
    template<typename T>
    using inline_ns_vector = detail::v1::vector_impl<T>;

    using inline_ns_string = detail::v1::basic_string<char>;
  }
}

using good_type = const int;
using array_type = int[24];

typedef void (*func)(int*);
template <func f> struct my_struct {};

void int_func(int*);

struct bad_type {};

template<typename T>
struct foo {};

typedef struct { } td_struct;
typedef enum { } td_enum;


typedef struct td_wrapped_struct { } inline_td_struct;
typedef enum td_wrapped_enum { } inline_td_enum;

int main() {
  { good_type                      var = bad_type{}; }
  { good_type*                     var = bad_type{}; }
  { good_type&                     var = bad_type{}; }
  { good_type&&                    var = bad_type{}; }
  { foo<good_type>                 var = bad_type{}; }
  { std::vector<int>               var = bad_type{}; }
  { std::string                    var = bad_type{}; }
  { std::inline_ns_vector<int>     var = bad_type{}; }
  { std::inline_ns_string          var = bad_type{}; }
  { not_std::vector<int>           var = bad_type{}; }
  { not_std::string                var = bad_type{}; }
  { not_std::inline_ns_vector<int> var = bad_type{}; }
  { not_std::inline_ns_string      var = bad_type{}; }
  { my_struct<&int_func>           var = bad_type{}; }
  { array_type                     var = bad_type{}; }
  { array_type*                    var = bad_type{}; }
  { array_type&                    var = bad_type{}; }
  { array_type&&                   var = bad_type{}; }
  { td_struct                      var = bad_type{}; }
  { td_enum                        var = bad_type{}; }
  { inline_td_struct               var = bad_type{}; }
  { inline_td_enum                 var = bad_type{}; }
  return 0;
}
