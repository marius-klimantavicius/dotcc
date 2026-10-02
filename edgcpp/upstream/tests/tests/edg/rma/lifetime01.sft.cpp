//options_all:-r -x -tused
//options: --strict;cp

static struct Iostream_init {
	Iostream_init() ; 
	~Iostream_init() ; 
} iostream_init ;	

struct Unit {
	static const Unit*
			lookup(char* u) ;
} ;
const Unit* meter = Unit::lookup("m") ;



