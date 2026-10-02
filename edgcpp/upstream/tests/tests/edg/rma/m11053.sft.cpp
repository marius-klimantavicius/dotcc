//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn

// #053 _113p41a can't adjust if derived has same mem name
class C {
protected:
	int i;
	struct C_nest {          // case 4: nested type
	// public by default
		int i;
		C_nest(int ii) : i(ii) { }
		};
	C_nest m;
public:
	int f1() { return m.i; }
	C(int ii) : i(ii) , m(ii) { }
	};

class D : private C {
protected:
	C::C_nest; // error - can't adjust, has same mem
public:
	struct C_nest {
	// public by default
		int i;
		C_nest(int ii) : i(ii) { }
		};
	D(int ii) : C(ii) { }
	};

void ign(const void *);
int main()
	{
	D o (11);
	ign(&o);
	return 0;
	}

