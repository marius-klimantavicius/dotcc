//remark:Complex type interpretation
//options:--gnu=60100;fp

struct complex
{
      constexpr complex(float __r = 0.0f)
      : _M_value{ __r} { }
      constexpr float
      real() const { return __real__ _M_value; }
     _Complex float  _M_value;
};
 
constexpr bool  operator==(const complex& __x, const complex& __y) { return __x.real(); }
const constexpr complex c1;
constexpr bool b = (c1 == c1);

auto z = c1;

