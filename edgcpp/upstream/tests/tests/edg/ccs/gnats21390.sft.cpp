//type:fn
//options_all:--c++20 -tused

template < class T > auto f ( T x ) ->
  decltype ( new auto [ x = 37 ] { ( T { } ; ( * new int [ ] { 0 } ) } T ( ) } ( { 0 } ) ) ) ;

void g()
{
  & f < int > {}
}
