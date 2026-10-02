//options_all:-r -x -tused
//options: --strict;rp


    int fun1() 
    { 
    return 1; 
    } 

    int main() 
    { 
    extern int fun1(); 

    class C1 { 
    friend int fun1(); 
    }; 

    if ((fun1()) != 1) { 
    return 6; 
    }  
    return 0; 
    } 

