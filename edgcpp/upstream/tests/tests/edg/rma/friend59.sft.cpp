//options_all:-r -x -tused
//options: --strict;cn:;cn

int g();  // forward declaration of global function g()

void f()
{
    struct local_class {
        local_class(){}
        ~local_class(){}
        friend int g()   // error : a global friend function cannot be
                         //        defined in a local class
        {
            return 1;
        }
    };
}

int main() { return 0; }


