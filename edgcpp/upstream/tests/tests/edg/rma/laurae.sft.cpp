//options_all:-r -x -tused
//options: --strict;cn:;rp

//// Teststate: A
// ==================== bad_c_enum_func.c ====================

extern "C" int printf(const char*, ...);

class A {
public:
	void bar();
};           

enum E {foo};  


void xxx(E e)      //// allow warning.*not used
{
	printf("xxx(E) called\n");
}

void xxx(int i)      //// allow warning.*not used
{
	printf("xxx(int) called\n");
}

int E(int i)      
{
	printf("E(int) called \n");
	return i;
}

void A::bar()
{
	E(37);			// no function call
	xxx(E(37));		// bogus call to E(int)
}

main()
{
	A a; 		//// allow warning.*not used

	a.bar();
	printf("xxx(int) called\n");
	return 0;
}


