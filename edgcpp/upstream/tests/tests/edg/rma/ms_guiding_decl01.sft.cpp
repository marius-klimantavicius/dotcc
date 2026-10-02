//options_all:-r -x -tused -n
//options:--microsoft_version=1200;cn:--microsoft_version=1000;cp

// Bug EDGqa00485

typedef int bool;

template<class _TYPE> inline
	bool operator>(const _TYPE& _X, const _TYPE& _Y)
	{return (_Y < _X); }


class C
{
};


bool operator>(const C&, const C&);


void f(const C &c1, const C &c2)
{
   if (c1 > c2)
   {
   }
}

