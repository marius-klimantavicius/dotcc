//options_all:-r -x -tused
//options: --strict;cn

/* C mode test of anonymous union extension */
typedef short DOC;
typedef long CP;
typedef struct _CA
	{
	CP cpFirst; CP cpLim; DOC doc;
	} CA;
struct _CALE;
typedef struct _CALE *PCALE;
typedef struct _CALE
	{
	union _CALEu
		{
		CA ca;
		CA;
		};
	PCALE pcaleNext;
	} CALE;
typedef struct _SELSF
	{
	union _SELSFu {
		struct _SELSFus
			{
			CP T1; CP cpLast; DOC T2;
			};
		CA;
		CA	ca;
		};
	} SELSF;
typedef struct _SELS
	{
	union _SELSu
		{
		SELSF;
		SELSF selsf;
		};
	} SELS;
typedef struct _SEL
	{
	union _SELu
		{
		SELS;
		SELS sels;
		};
	} SEL;


void fn()
{
        CA x1;
        SELSF x2;
        SELS x3;
	SEL eelsel;
        x1.doc = 0;
        x2.doc = 0;
        x3.doc = 0;
	eelsel.doc = 0;
}


