//options_all:-r -x -tused
//options: --strict;cn

struct X
	{
	int i;
	X(int a, int b) : i(a + b) { }
	};

int main()
	{
	X x = X(0, 1, 2);
	return x.i;
	}

