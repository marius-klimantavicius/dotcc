//type:fp
//options:--c++20:--c++20 --no_exceptions

#include <coroutine>

namespace minimal
{
  //#include <coroutine>
  struct C {
    ~C();
  };
  std::task<int> coro() {
    C c1, c2;
    co_return 0;
  }
}

namespace simple_object
{
  struct C
  {
    C();
    ~C();
  };

  std::task<int> coro()
  {
    C c;
    co_return 0;
  }
}

namespace two_objects
{
  struct C
  {
    C();
    ~C();
  };

  std::task<int> coro()
  {
    C c1;
    C c2;
    co_return 0;
  }
}

namespace several_objects_in_separate_scopes
{
  struct C
  {
    C();
    ~C();
  };

  std::task<int> coro(bool b)
  {
    C c1;
    {
      C c2;
      if (b)
      {
        C c3;
        co_return 0;
      }

      C c4;
    }

    C c5;

    co_return 0;
  }
}
