//options_all:-r -x -tused
//options: --strict;cn

// #011 _141p32b template decl may appear only as global decl

class cl
	{
	template <class T> class x { T i; }; // error - not global decl
	}

