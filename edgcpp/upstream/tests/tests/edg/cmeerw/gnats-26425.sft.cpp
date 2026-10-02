//type:fn
//options:--c++11:--c++17:--c++20:--ms_c++20

namespace minimal {
  struct R {
    const int *begin(), *end();
  };
  void f(R r) {
    for (struct S { S(int) { } } s : r) { } // type definition is not allowed
  }
}

struct R
{
  int *begin() const;
  int *end() const;
};

void for_cond(R r)
{
  // type definition is not allowed
  for (; struct S { S(int) { } operator bool() const { return false; } } e = 1; )
  { }
}

void for_init(R r)
{
  struct S
  {
    S(int)
    { }

    operator bool() const
    {
      return false;
    }
  };

  for (struct C { } c; struct S e = 1; )
  {
    (void) c;
  }
}

void range_for_decl(R r)
{
  for (struct S { S(int) { } } e : r) // type definition is not allowed
  { }
}

#if __cplusplus >= 202002L
void range_for_init(R r)
{
  struct S
  {
    S(int) { }
  };

  for (struct C { } c; struct S e : r)
  {
    (void) c;
  }
}
#endif

void if_cond()
{
  // type definition is not allowed
  if (struct S { S(int) { } operator bool() const { return false; } } e = 1)
  { }
}

#if __cplusplus >= 201703L
void if_cond_init()
{
  struct S
  {
    S(int)
    { }

    operator bool() const
    {
      return false;
    }
  };

  if (struct C { } c; struct S e = 1)
  {
    (void) c;
  }
}
#endif

void switch_cond()
{
  // type definition is not allowed
  switch (struct S { S(int) { } operator int() const { return 0; } } e = 1)
  { }
}

#if __cplusplus >= 201703L
void switch_cond_init()
{
  struct S
  {
    S(int)
    { }

    operator int() const
    {
      return 0;
    }
  };

  switch (struct C { } c; struct S e = 1)
  {
    case 0:
    (void) c;
  }
}
#endif

void while_cond()
{
  // type definition is not allowed
  while (struct S { S(int) { } operator bool() const { return false; } } e = 1)
  { }
}

void storage_class(int (&arr)[2])
{
  for (static int i : arr) { }       // error: storage class
  for (extern int i : arr) { }       // error: storage class
  for (mutable int i : arr) { }      // error: storage class
  for (thread_local int i : arr) { } // error: storage class
}

void constexpr_range()
{
  static constexpr int arr[] { 1, 2 };
  for (constexpr int i : arr) { }
}
