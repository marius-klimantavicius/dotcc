//type: fp
//options: 
# 0 "./lto/20081125_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20081125_1.C"
# 1 "./lto/20081125.h" 1
class base
{
 public:
 base() {}
 virtual ~base() {}
 static base *factory (void);
};

class object : public base
{
 public:
 object() {}
 object (int);
 virtual void key_method (void);
};
# 2 "./lto/20081125_1.C" 2

base *
base::factory(void)
{
 return new object ();
}
