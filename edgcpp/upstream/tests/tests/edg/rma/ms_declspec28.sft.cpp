//options_all:-r -x -tused
//options: --microsoft -n;cp

template<class T> class X { };
extern template class __declspec(dllimport) X<char>;

