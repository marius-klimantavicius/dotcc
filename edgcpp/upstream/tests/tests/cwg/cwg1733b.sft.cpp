//type:fp
//options_all:--c++20 -tused -A
    struct A {
      A& operator=(A const&) && = default;
    };

