//remark:Exception specs and explicit specializations
//options:--gnu=70300 --c++17;fp:--c++20 -A;fp

template<typename> void f() noexcept(true);

template<> void f<int>() noexcept;
