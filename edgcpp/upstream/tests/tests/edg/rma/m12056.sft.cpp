//options_all:-r -x -tused
//options: --strict;cn

#include <stddef.h>

char buf[1024];

struct X
	{
	virtual void *operator new(size_t n) // error - can't be virtual
		{
		return n < sizeof(buf) ? buf : 0;
		}
	};

int main()
	{
	return (new X) == 0;
	}


