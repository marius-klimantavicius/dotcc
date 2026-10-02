//options_all:-r -x -tused
//options: --strict;cn

void* operator new ((unsigned int s , int i) 
{
        return 0;
}

struct A {
        A() {throw -37;}
};


