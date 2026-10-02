//type:cp
//options_all:--microsoft_version 1928

class IRTTIObject
{
public:
  using ClassCRC = int;
};


template<typename T>
class IExpression;

template<typename T, typename A>
class BinaryOperators;

template<typename T>
class ComparisonOperators;



template<typename T>
class IExpression : public IRTTIObject
{
};

template<typename T, typename A>
class BinaryOperators : public IExpression<T>
{
public:
  using typename IExpression<T>::ClassCRC;
};

template<typename T>
class ComparisonOperators : public BinaryOperators<bool, T>
{
};
