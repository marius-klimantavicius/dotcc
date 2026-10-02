//remark:Complex type interpretation
//options:--gnu=60100;fp

namespace std
{
template <typename _Tp> class complex;
template <> class complex<float>;
template <> struct complex<float> {
typedef __complex__ float _ComplexT;
constexpr complex()
: _M_value(0)
{
}
private:
_ComplexT _M_value;
};
}
constexpr std::complex<float> f;
std::complex<float> g = f;
