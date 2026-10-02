//type:fp
//options:--c++17:--c++17 --g++:--ms_c++17;fn

namespace extend_from_inline_namespace
{
  inline namespace inl
  {
    namespace ns
    {
      int foo();
    }
  }

  namespace ns
  {
    void bar()
    {
      auto v = foo();
    }
  }
}

namespace extend_from_inline_namespace_in_extension
{
  namespace outer
  {
    inline namespace inl
    {
      namespace ns
      {
        void foo();
      }
    }
  }

  namespace outer
  {
    namespace ns
    {
      void bar()
      {
        foo();
      }
    }
  }
}

namespace extend_from_nested_inline_namespaces
{
  inline namespace inl1
  {
    inline namespace inl2
    {
      namespace ns { int foo(); }
    }
  }

  namespace ns
  {
    auto v = foo();
  }
}

namespace extend_from_inline_namespace_with_decl_in_namespace
{
  inline namespace inl1
  {
    namespace ns { int foo(); }
  }

  struct ns { };

  namespace ns
  {
    auto v = foo();
  }
}

namespace extend_from_inline_namespace_with_decl_in_other_inline_namespace_1
{
  inline namespace inl1
  {
    namespace ns { int foo(); }
  }

  inline namespace inl2 {
    struct ns { };
  }

  namespace ns
  {
    auto v = foo();
  }
}

namespace extend_from_inline_namespace_with_decl_in_other_inline_namespace_2
{
  inline namespace inl1
  {
    struct ns { };
  }

  inline namespace inl2
  {
    namespace ns { int foo(); }
  }

  namespace ns
  {
    auto v = foo();
  }
}

namespace ignore_non_ns_in_inline_namespace
{
  inline namespace inl1
  {
    struct ns { };
  }

  namespace ns
  { }
}

namespace dont_extend_from_using_namespace
{
  namespace non_inl
  {
    namespace ns
    {
      void foo() { }
    }
  }

  using namespace non_inl;

  namespace ns
  {
    void foo() { }
  }
}

namespace dont_extend_from_used_inline_namespace
{
  namespace outer
  {
    inline namespace inl
    {
      namespace ns
      {
        void foo() { }
      }
    }
  }

  using namespace outer::inl;

  namespace ns
  {
    void foo() { }
  }
}

namespace namespace_alias_doesnt_look_up_in_inline_set
{
  inline namespace inl {
    namespace ns { }
  }

  namespace ns2 { }
  namespace ns = ns2;
}
