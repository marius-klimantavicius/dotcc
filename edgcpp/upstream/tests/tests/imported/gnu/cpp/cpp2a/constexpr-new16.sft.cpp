//type: fp
//options: --c++20
// PR c++/98988
// { dg-do compile { target c++20 } }
// { dg-options "-fno-delete-null-pointer-checks" }

constexpr bool
foo ()
{
  auto ptr = new int();
  delete ptr;
  return true;
}

static_assert (foo ());
