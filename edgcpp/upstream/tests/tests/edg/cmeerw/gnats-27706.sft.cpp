//type:fp
//options:--c++11 -d-hash_stats
//filter:awk '/^Hash statistics for: "D"/{f=1; print $0; next}/^Total entries=/{next}/^    [0-9]:/{next}/^Hash statistics for: /{f=0; next}f'

// testing note: hash statistics for counts < 16 are filtered, shouldn't then
// show any larger counts

template<int I>
struct C { };

template<int I>
struct D { };

template<int I>
struct F : F<I - 1>
{
  template<int J>
  void f(D<C<I + J>::v1>, D<C<I + J>::v2>,
      D<C<I + J>::v3>, D<C<I + J>::v4>,
      D<C<I + J>::v5>, D<C<I + J>::v6>,
      D<C<I + J>::v7>, D<C<I + J>::v8>,
      D<C<I + J>::v9>, D<C<I + J>::v10>,
      D<C<I + J>::v11>, D<C<I + J>::v12>,
      D<C<I + J>::v13>, D<C<I + J>::v14>,
      D<C<I + J>::v15>, D<C<I + J>::v16>,
      D<C<I + J>::v17>, D<C<I + J>::v18>,
      D<C<I + J>::v19>, D<C<I + J>::v20>)
  { }

  template<int J>
  void g(D<C<I + J + 1>::v>, D<C<I + J + 2>::v>,
      D<C<I + J + 3>::v>, D<C<I + J + 4>::v>,
      D<C<I + J + 5>::v>, D<C<I + J + 6>::v>,
      D<C<I + J + 7>::v>, D<C<I + J + 8>::v>,
      D<C<I + J + 9>::v>, D<C<I + J + 10>::v>,
      D<C<I + J + 11>::v>, D<C<I + J + 12>::v>,
      D<C<I + J + 13>::v>, D<C<I + J + 14>::v>,
      D<C<I + J + 15>::v>, D<C<I + J + 16>::v>,
      D<C<I + J + 17>::v>, D<C<I + J + 18>::v>,
      D<C<I + J + 19>::v>, D<C<I + J + 20>::v>)
  { }
};

template<>
struct F<0>
{ };

F<60> f;
