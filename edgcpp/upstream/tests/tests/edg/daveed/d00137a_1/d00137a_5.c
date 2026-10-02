//file
extern "C" void exit(int);

template <class T = short> struct A {
	static int x;
	static int y;
};

template <> int A<double>::x = 37;

template <> int A<>::y = 47;

int main()
{
	if (A<double>::x + A<short>::y != 84)
		exit(1);

	exit(0);

	return 0;
}
