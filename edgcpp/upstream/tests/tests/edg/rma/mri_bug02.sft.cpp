//options_all:-r -x -tused
//options: --strict;cn

static union { int i; float f; } ;                                             
static union { int j; float g; } = {1}; // bug, Not allowed                    
int i = 1; // Not allowed, not bug                                             


