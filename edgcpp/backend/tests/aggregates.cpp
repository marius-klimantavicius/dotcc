struct Pair { int a; long b; };
struct Nested { unsigned char tag; Pair pair; int values[3]; Pair *next; };
union Word { unsigned int u; unsigned char bytes[4]; };
#pragma pack(push, 1)
struct Packed { unsigned char tag; int value; };
#pragma pack(pop)
Packed packed[2] = {{3, 101}, {5, 103}};
struct Node { int value; Node *next; };
int values[4] = { 2, 3, 5, 7 };
int matrix[2][3] = {{1, 2, 3}, {4, 5, 6}};
Pair pairs[2] = {{3, 11}, {5, 17}};
Nested object = { 9, { 13, 19 }, {23, 29, 31}, &pairs[1] };
int *interior = &object.values[1];
Word word = {0x12345678U};
Node node = { 47, &node };
int sum(const int *p, int count) { int n=0; for(int i=0;i<count;++i) n+=p[i]; return n; }
int inspect(Pair p) { p.a += 1; return p.a + (int)p.b; }
Pair make(int n) { Pair p = { n, 41 }; return p; }
struct Bundle { int a[4]; Pair p[2]; };
Bundle transfer(Bundle b) { b.a[3]+=1; b.p[1].b+=2; return b; }
int main() {
    if (sum(values,4)!=17 || values+4-values!=4) return 1;
    if (matrix[1][2]!=6 || matrix[1]-matrix[0]!=3) return 2;
    if (object.pair.a!=13 || object.values[2]!=31 || object.next->b!=17) return 3;
    if (*interior!=29 || interior!=&object.values[1]) return 4;
    if (reinterpret_cast<unsigned long>(&object.pair)-reinterpret_cast<unsigned long>(&object)!=8) return 5;
    if (sizeof(Pair)!=16 || sizeof(Nested)!=48 || sizeof(matrix)!=24) return 6;
    int local[5] = { 1, 2 };
    if (local[0]!=1 || local[1]!=2 || local[4]!=0) return 7;
    Pair p = {7, 23};
    Pair copy = p;
    copy.a = 9;
    if (p.a!=7 || copy.a!=9 || copy.b!=23) return 8;
    p = copy;
    if (p.a!=9 || inspect(p)!=33 || p.a!=9) return 9;
    Pair result = make(37);
    if (result.a!=37 || result.b!=41) return 10;
    if (node.next!=&node || node.next->value!=47) return 11;
    unsigned char *bytes = reinterpret_cast<unsigned char*>(&word);
    if (bytes[0]!=0x78 || bytes[3]!=0x12) return 12;
    word.bytes[0]=0x55;
    if(word.bytes[0]!=0x55 || sizeof(Word)!=4) return 15;
    if(sizeof(Packed)!=5 || packed[1].value!=103 ||
       reinterpret_cast<unsigned long>(&packed[1])-reinterpret_cast<unsigned long>(&packed[0])!=5) return 16;
    packed[1].value += 4;
    if(packed[1].value!=107) return 17;
    signed char small=-3;
    unsigned short u=65535;
    short n=-123;
    ++u;
    if(small!=-3 || u!=0 || n!=-123) return 18;
    alignas(64) Pair aligned[2];
    aligned[1].a=53;
    if (reinterpret_cast<unsigned long>(aligned)%64 || aligned[1].a!=53) return 13;
    // Declarations inside loops must reuse the bounded function frame.
    for (int i=0;i<100000;++i) { int block[32]={}; block[31]=i; if(block[31]!=i) return 14; }
    Bundle bundle={{2,3,5,7},{{11,13},{17,19}}};
    Bundle moved=transfer(bundle);
    if(bundle.a[3]!=7 || bundle.p[1].b!=19 || moved.a[3]!=8 || moved.p[1].b!=21) return 19;
    if(moved.a[0]!=2 || moved.a[2]!=5 || moved.p[0].b!=13 || moved.p[1].a!=17) return 20;
    return 0;
}
