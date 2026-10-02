//options_all:-r -x -tused --diag_suppress=2458
//options: --strict;cn

static union {
	int* x;
	union {
		typedef int x;
	};
};

