//options_all:-r -x -tused
//options: --strict;cn:;cp

enum E {};
template <class T> struct C {
	enum T x;
};
C<E> ce;

