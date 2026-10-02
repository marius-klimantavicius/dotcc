//type: fp
//options: 
struct A
{
  int membervar;
};

typedef const A type;

int type::* getmemberptr() { return &type::membervar; }
