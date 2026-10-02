//options_all:-r -x -tused
//options: --strict;cp

// EDGqa01349, EDGqa01352 -- spurious error
struct ModelItem 
{
        virtual void F () {;}
};

struct Parameter 
{
        virtual void F () {;}
};
    
struct ParamItem : public virtual ModelItem, public virtual Parameter 
{
        virtual  void F () {;}
};
    
struct ParamItem_tk : public virtual ParamItem, public virtual ModelItem , public virtual Parameter {};
    
int main ()
{
  ParamItem_tk	P;
  P.F ();	
}


