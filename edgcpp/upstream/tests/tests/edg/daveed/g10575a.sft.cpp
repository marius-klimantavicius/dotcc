//remark:Extern functions depending on types with no linkage
//options:--microsoft_version=1310;fp:--microsoft_version=1400;fp:--microsoft_version=1600;fp

typedef struct {} * T1;
void g(T1);
typedef struct {} * T2;
void g(T2);

void f(T1 p1, T2 p2)
{
        g(p1);
        g(p2);
}

int main()
{
        f(0, 0);
}
