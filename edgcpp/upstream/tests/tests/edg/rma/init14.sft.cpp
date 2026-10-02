//options_all:-r -x -tused
//options: --strict;cp

struct S {
	int member;

	S ();
};

S::S () : member() { }		/* ERROR */

int main () { return 1; }

