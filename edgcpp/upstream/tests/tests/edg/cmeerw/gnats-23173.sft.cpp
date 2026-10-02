//type:fp
//options:--c++23:--c++23 --microsoft

template<bool B>
struct C;

template<>
struct C<true>
{ };

template<bool B>
constexpr void foo()
{
  if constexpr (!B)
    int{}, C<B>();

  if constexpr (B)
  { }
  else
    int{}, C<B>();

  if constexpr (B)
    ;
  else
    int{}, C<B>();

#ifdef __cpp_consteval
  if constexpr (!B)
    if consteval
    { }
    else
    { }

  if constexpr (!B)
    if ! consteval
    { }
    else
    { }

#ifndef _MSC_VER
  if constexpr (!B)
    if not consteval
    { }
    else
    { }
#endif

  if constexpr (B)
    ;
  else
    if consteval
    { }
    else
    { }

  if constexpr (B)
    ;
  else
    if ! consteval
    { }
    else
    { }

#ifndef _MSC_VER
  if constexpr (B)
    ;
  else
    if not consteval
    { }
    else
    { }
#endif
#endif
}

template void foo<true>();
