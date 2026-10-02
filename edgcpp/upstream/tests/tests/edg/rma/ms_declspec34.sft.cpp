//options_all:-r -x -tused --no_wchar_t
//options: --microsoft --microsoft_version=1310 -n;cp

typedef int wchar_t;
template<class _P> struct basic_ostream { };
template<class _P> basic_ostream<_P>& operator<<(basic_ostream<_P>&, int) { };
template<> __declspec(dllimport) basic_ostream<char>& operator<<(basic_ostream<char>&, int);
template<> basic_ostream<wchar_t>& __declspec(dllimport) operator<<(basic_ostream<wchar_t>&, int);

main() {
  basic_ostream<char> bos_char;
  basic_ostream<wchar_t> bos_wchar_t;

  bos_char << 1;
  bos_wchar_t << 1;
}

