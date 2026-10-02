//remark:Complex type interpretation
//options:--gnu=60100;fn:--gnu=80100;fp

struct complex
{
  constexpr complex(long double __r = 0.0L, long double __i = 0.0L) : _M_value{ __r, __i } { }
  __complex__ long double _M_value;
};
 
constexpr complex  operator""il(long double __num)
{ return complex  {0.0L, __num}; }
 
const constexpr complex c5 ( 0.5il );
complex x = c5;
