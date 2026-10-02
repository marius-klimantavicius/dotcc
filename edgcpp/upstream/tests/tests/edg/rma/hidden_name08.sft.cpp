//options_all:-r -x -tused
//options: --strict;cp

template <class Type > void
SortResults( const Type * ) {
     
}

class List {
public:
  void SortResults();
};

void
List::SortResults()
{
  ::SortResults( (List*) 0 );
}


