//type:fp
//options_all:--microsoft_version 1900 --set_flag coroutines -tused

//#define coroutine_traits resumable_traits
//#define coroutine_handle resumable_handle

namespace std {
  namespace experimental {

    template<typename T1, typename... Ts> struct first_type {
      using type = T1;
    };

    template<typename... Ts> struct coroutine_traits {
      using promise_type = typename first_type<Ts...>::type::promise_type;
    };

    template<typename PromiseT = void> struct coroutine_handle {};

    template<class T, int N = 1> struct task {
      struct promise_type {
        void return_value(T);
      };
    };
  }
}

using std::experimental::coroutine_handle;
using std::experimental::task;

struct A {
};

bool await_ready(A const&);
void* await_suspend(A, coroutine_handle<task<int>::promise_type>);
#ifdef NEG
bool await_resume(A const&&);
#else
bool await_resume(A&);
#endif

int g(int);
A h();

template<class T> auto f(T) {
  return g(__await h());
}

