//remark:Initialization of flexible array members
//options:--gcc;fn

struct fa_st {
  int no_of_elem;
  int data[];
};

typedef struct fa_st fa_st_t;

int main()
{
  fa_st_t obj = {4,{10,20,30,40}};
  return obj.no_of_elem;
}

