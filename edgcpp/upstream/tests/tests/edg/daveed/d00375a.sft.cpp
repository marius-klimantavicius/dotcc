//remark:GNU template attributes
//type:fp
//name:
//options:
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

class __attribute__ ((__deprecated__)) Deprecated
{
public:
    int member;
};

template <typename T>
class Template
{
public:
    int member;
};

template <typename T>
class __attribute__ ((__deprecated__)) DeprecatedTemplate
{
public:
    int member;
};

template <typename T>
class __attribute__ ((__deprecated__)) DeprecatedDerivedTemplate :
public Template<T>
{
public:
    int member;
};

template <typename T>
class __attribute__ ((__deprecated__))
DeprecatedDerivedDeprecatedTemplate : public DeprecatedTemplate<T>
{
};

int main(int, char **)
{
    Deprecated a;
    DeprecatedTemplate<int> b;
    DeprecatedDerivedTemplate<int> c;
    DeprecatedDerivedDeprecatedTemplate<int> d;

    a.member = 0;
    b.member = 0;
    c.member = 0;
    d.member = 0;

    return 0;
}


