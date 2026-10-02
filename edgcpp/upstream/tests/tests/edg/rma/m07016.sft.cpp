//options_all:-r -x -tused
//options: --strict;cn

class X;
extern X a;
extern X &f(int);

int main()
	{
	a = f(0); // error - can't use object of unknown class
	return 0;
	}

