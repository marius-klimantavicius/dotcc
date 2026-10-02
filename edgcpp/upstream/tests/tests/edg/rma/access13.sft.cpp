//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

class Global {
	int privateMemberOfGlobal;

public:
	void memberFunctionOfGlobal();
};

void
Global::memberFunctionOfGlobal() {
	struct local {
		void f( Global *global ) {
			global->privateMemberOfGlobal = 0; // legal?
		}
	} l;

	l.f( this );
}

