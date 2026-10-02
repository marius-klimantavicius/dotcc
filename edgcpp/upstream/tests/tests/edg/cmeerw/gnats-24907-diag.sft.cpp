//type:fn
//options:--c++20:--ms_c++20 --microsoft_version 1938
//options_all:-tused

// there are still memory region issues later on, so at least generate an error
// and check for that

template <int I>
struct C
{
  int f()
  {
    [ ] ( auto x ) {
      static_assert( requires { this; } );
      static_assert( requires { this + 1; } );
      static_assert( requires { * this; } );
      static_assert( requires { * this + 1; } ); // error
    } ( 1 );
    return 1;
  }
};

int x = C<1>().f();
