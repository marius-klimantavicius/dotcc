//type:cp
//options:--c++11:--c++17:--gnu_version 70400:--gnu_version 70400 --c++17:--microsoft_version 1920:--microsoft_version 1920 --c++17

struct begin {
  template <typename Untagged, typename Next>
    class getter : public Next {
  public:
    getter() = default;
    using Next::Next;
    template <bool T = false>
      constexpr getter(Untagged const &that) noexcept(false);
  };
};

struct end {
  template <typename Untagged, typename Next>
    class getter : public Next {
  public:
    getter() = default;
    using Next::Next;
    template <bool T = false>
      constexpr getter(Untagged const &that) noexcept(false);
  };
};

class Foo {};

begin::getter<Foo, end::getter<Foo, Foo>> foo;
