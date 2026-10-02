//remark:Explicit default constructors
//options:--c++14;fn:--microsoft_v=1924;fp

class Bar
{
public:
   explicit Bar(float f = 0.0F)
   {
   }
};
struct Foo
{
   Bar a;
};
Foo foo = {};
