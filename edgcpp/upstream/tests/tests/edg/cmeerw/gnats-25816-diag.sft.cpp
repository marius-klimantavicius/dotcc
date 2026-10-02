//type:fn
//options:--c++17:--c++17 --g++:--ms_c++17

namespace redeclaration_with_inline_namespace
{
  struct ns { };

  inline namespace inl {
    struct ns { };
  }

  namespace ns { }              // error: already defined
}

namespace ambiguous
{
  namespace ns { void foo() { } }

  inline namespace inl1 {
    namespace ns { void bar() { } }
  }

  namespace ns {                // ambiguous
    void foo() { }              // error: already defined
    void bar() { }
  }
}

namespace ambiguous_inline {
  inline namespace inl1 {
    namespace ns { void foo() { } }
  }

  inline namespace inl2 {
    namespace ns { void bar() { } }
  }

  namespace ns { }              // ambiguous
}

namespace alias_in_inline {
  inline namespace inl {
    namespace ns2 { }
    namespace ns = ns2;
  }

  namespace ns { void foo() { } } // conflicts with inl::ns
}
