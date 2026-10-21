constexpr float m(float x,float y){return x*y;}constexpr float inf=m(0x1p127f,2.0f);static_assert(inf>0x1.fffffep127f);
