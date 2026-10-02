//type: fp
//options: 
// PR c++/18369

void breakme () 
{
  int *v = new (int [5]);
}
