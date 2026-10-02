//options_all:-r -x -tused
//options: --strict;cn:;rp

namespace N {
	int i;
	struct j {};
};

namespace N {
	struct i {};
	int j;
};

int main()
{
	int j;
	j = N::i;
	j = N::j;
}

