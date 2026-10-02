//options_all:-r -x -tused
//options: --strict;cn:;cp

typedef enum {
	PhoneHome,
} MyEnum;

class ET {
public:
	ET(MyEnum);
	char * val(void);
};

int
main(void)
{
	ET e(PhoneHome);
}


