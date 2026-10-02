//options_all:-r -x -tused
//options: --strict;cn:;cn

int f(int * far p) { return *p; };
int f(int * near p) { return *p; };


