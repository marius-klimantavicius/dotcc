//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;rp:;rp
//filter:sed -e 's/EH stack entry at 0x[0-9a-f]*/EH stack entry at 0xXXXXXXXX/g'

// check for use of goto within try/catch

extern "C" void exit(int);

void f()
{
	throw 1;
}

extern int __debug_level;

int main()
{
	int x;
	int flag = 0;
	__debug_level = 5;
	try {
		x = 1;
loop1:
		x++;
		if (x < 10)
			goto loop1;
		f();
	}
	catch (int d) {
		flag = 1;
		x = 1;
loop2:
		x++;
		if (x < 10)
			goto loop2;
	}
	if (!flag)
		exit(1);
	exit(0);
}


