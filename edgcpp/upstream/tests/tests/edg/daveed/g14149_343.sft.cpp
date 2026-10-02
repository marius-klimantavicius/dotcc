//remark:Complex type interpretation
//options:--gnu=60100;fp

namespace std {
  template<typename _Tp> class complex;
  template<> class complex<float>;


  template<>
    struct complex<float>
    {
      typedef float value_type;
      typedef __complex__ float _ComplexT;
      constexpr complex(_ComplexT __z) : _M_value(__z) { }
      constexpr complex(float __r = 0.0f, float __i = 0.0f)
      : _M_value{ __r, __i } { }

      constexpr _ComplexT __rep() const { return _M_value; }

    private:
      _ComplexT _M_value;
    };
}


int main(void)
{
constexpr std::complex<float> x00(1.1f);

}
