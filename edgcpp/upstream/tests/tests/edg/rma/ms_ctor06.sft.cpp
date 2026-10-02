//options_all:-r -x -tused
//options: --microsoft -n;cp

template< class T1, class T2 >
class pair
  {
  };

template < class _Key, class _Value >
class __UTxKeyValuePair : public pair< _Key,_Value >
{
     typedef __UTxKeyValuePair< _Key,_Value >  _Mty;
public:
     _Mty() { }
};

