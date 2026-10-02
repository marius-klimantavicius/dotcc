//type: fn
//options:  --c++11
// PR c++/60186
// { dg-require-effective-target c++11 }

template<typename> void foo(int i)
{
  constexpr int a[] = { i };	// { dg-error "" }
}
