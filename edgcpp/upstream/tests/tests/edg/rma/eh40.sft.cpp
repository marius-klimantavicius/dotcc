//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;rp

#ifdef __APPLE__
typedef __EDG_SIZE_TYPE__ size_t;
#else
typedef __EDG_SIZE_TYPE__ size_t;
#endif
extern "C" void exit(int);

void operator delete(void* p, int i) 
{
	if (i == 0)
		exit(0);
	else
		exit(1);
}

void* operator new(size_t s, int i = -59)
{
	return new char[s];
}

struct A {
	A() {throw -37;}
};

int main()
{
	new (0) A;
	exit(1);
}

