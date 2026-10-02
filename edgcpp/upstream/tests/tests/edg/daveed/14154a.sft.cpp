//remark:Rendering of __is_nothrow_assignable
//options:--c++11;fp

    struct A {
      A() noexcept;
    };
    static_assert(__is_nothrow_assignable(A, A), "XXX");

