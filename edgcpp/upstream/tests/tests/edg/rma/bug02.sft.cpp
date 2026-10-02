//options_all:-r -x -tused
//options: --strict;cn

typedef struct _CA
	{
        short doc;
	} CA;
typedef struct _SELSF
	{
	union _SELSFu {
		CA;
		};
	} SELSF;
typedef struct _SELS
		{
		SELSF;
	} SELS;

void fn()
{
        SELSF x2;
        x2.doc = 0;
}


