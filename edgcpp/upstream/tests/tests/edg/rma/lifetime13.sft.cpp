//options_all:-r -x -tused
//options: --strict;cp

struct A { A(); ~A(); };
void f() {
  try { } catch (int) {
    {
    A a;
    goto yyy;
    }
yyy:;
  }
}
/*
// check use of labels with EH

extern "C" void exit(int);

int ctor = 0;
int dtor = 0;

struct A {
	A() {ctor++;}
	~A() {dtor++;}
};

void f()
{
	int x;

	x = 17;
	if (x) {
		A a;
		goto xxx;
	}
	return;
xxx:
	try {
		A a;
		throw 59;
	}
	catch (int i) {
		if (x) {
			A a;
			goto yyy;
		}
		return;
yyy:;
		throw;
	}
}

main()
{
	int flag = 0;
	int cnt = 0;

xxx:
	try {
		A a;
		f();
	}
	catch (int i) {
		if (i != 59)
			exit(1);
		flag++;
	}
	if (++cnt < 10) {
		A a;
		goto xxx;
	}

	if (flag != 10)
		exit(2);

	if (ctor != 49 || ctor != 49)
		exit(3);

	exit(0);
}

*/

