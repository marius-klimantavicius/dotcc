//options_all:-r -x -tused
//options: --microsoft_version=1200 -n;cp

//options_all:--microsoft
//type:cp
// line 70 in iosfwd
template<class _E>
        struct char_traits {
        typedef _E char_type;
        typedef _E int_type;
        // more...
        };

template<class _E, class _Tr = char_traits<_E> >
        class basic_istream;

// line 265 in iosfwd
extern template __declspec(dllimport) basic_istream<char, char_traits<char> >& __cdecl operator>>(
       basic_istream<char, char_traits<char> >&, char *);

