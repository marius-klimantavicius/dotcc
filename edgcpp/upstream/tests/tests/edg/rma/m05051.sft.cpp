//options_all:-r -x -tused
//options: --strict;cn

void ignore(void *);
int main()
	{
	class X
		{
		double d1, d2;
		X(double dd1, double dd2) : d1(dd1), d2(dd2) { }
		};

	X x = (X)0.0;
	ignore(&x);
	return 0;
	}

