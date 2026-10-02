//remark:Null pointer constants
//options:--c++11;fp

static constexpr signed char OFF = 0x00;
static constexpr signed char ON = 0x01;

void f(char);
void f(const char*);

void f()
{
   f(ON);
   f(OFF);
}
