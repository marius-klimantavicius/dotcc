//options_all:-r -x -tused
//options: --strict;cn

// m09035 -- see 92p_212c
class x {
public:
	union { int x; char c; }; // error - same name as class
	int i;
	};
x xa;

