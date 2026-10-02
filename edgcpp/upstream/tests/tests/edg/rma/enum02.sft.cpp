//options_all:-r -x -tused
//options: --strict;cp

class ErrSpecs {
public:
        int severity;            
        char* args;              
        char* text;              
        };
class ErrFac {
public:
typedef enum { 
        SUCCESS = 0,
        INFORMATION = -1,
        INFO = -1,
        WARNING = -2,
        ERROR = -3,
        FATAL = -4,
        DEFAULT = 1 } severity_level;
        char* longname;          
        ErrSpecs* errlist;       
        int last;                
};

