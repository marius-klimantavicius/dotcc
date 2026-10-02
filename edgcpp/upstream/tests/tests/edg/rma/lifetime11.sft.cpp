//options_all:-r -x -tused
//options: --strict;rp

extern "C" int printf(char*, ...);
int main()
{
	int i = 1;
	switch (i) {
		case 1:
			i = 2;
		case 2:
			i = 3;
	};
        printf("%d %s 3\n", i, i==3 ? "==" : "!=");
}


