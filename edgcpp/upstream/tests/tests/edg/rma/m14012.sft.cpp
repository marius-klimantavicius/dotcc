//options_all:-r -x -tused
//options: --strict;cn

// #012 _141p32c template decl may appear only as global decl

class cl
	{
	template <class T> T f(T i) // error - not global decl
		{
		return i;
		}
	int j;
	}

