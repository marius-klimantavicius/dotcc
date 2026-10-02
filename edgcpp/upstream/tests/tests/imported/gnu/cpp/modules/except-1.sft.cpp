//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
export module bill;
// { dg-module-cmi bill }

// Make sure no deferred parse exception spec detritus remains

template<typename T>
class bob 
{
  void frob () noexcept(T::frob ());
  template<typename U> void frobber (int) noexcept (T::frob ());
};


class bill 
{
  template<typename U> void frobbest (int) noexcept (U::frob ());
};
