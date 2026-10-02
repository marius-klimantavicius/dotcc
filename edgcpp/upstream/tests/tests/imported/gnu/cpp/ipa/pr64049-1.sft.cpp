//type: fp
//options: 
# 0 "./ipa/pr64049-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ipa/pr64049-1.C"



# 1 "./ipa/pr64049.h" 1


typedef unsigned int EnumStatusCode;

class StatusCode
{
public:
 static const EnumStatusCode ERROR = 0x8000;
 static const EnumStatusCode SUCCESS = 0x0000;
 static bool isSUCCEEDED (EnumStatusCode res) { return (res == SUCCESS); }
};

class LocalizedTextStruct
{
public:
 LocalizedTextStruct () {}
 LocalizedTextStruct (const char *val)
 {
  __builtin_strcpy (t, val);
 }
 char *getT () { return t; }
private:
 char t[99];
};

typedef union tagValueUnion
{
 LocalizedTextStruct* LocalizedText;
} ValueStructUnion;

typedef struct ValueStruct
{
 unsigned char arrayType;
 unsigned short dataType;
 ValueStructUnion value;
} ValueStruct;

class LocalizedText
{
public:
 virtual LocalizedTextStruct* getInternHandle ();
private:
 LocalizedTextStruct t;
};

class ValueHelper
{
public:
 static EnumStatusCode getLocalizedText (const ValueStruct* pValueStruct, LocalizedText& target);
 static LocalizedText getLocalizedText (const ValueStruct* pValueStruct);
};

EnumStatusCode LocalizedTextSet (LocalizedTextStruct* pTarget, LocalizedTextStruct* pSource);
# 5 "./ipa/pr64049-1.C" 2

EnumStatusCode
ValueHelper::getLocalizedText (const ValueStruct* pValueStruct, LocalizedText& target)
{
 if (pValueStruct && pValueStruct->dataType == 0 && pValueStruct->arrayType == 0)
 {
  { if (!(pValueStruct->value.LocalizedText)) __builtin_abort (); } (void)0;
  return LocalizedTextSet (target.getInternHandle (), pValueStruct->value.LocalizedText);
 }
 else
 {
  return StatusCode::ERROR;
 }
}

LocalizedText
ValueHelper::getLocalizedText (const ValueStruct* pValueStruct)
{
 LocalizedText returnValue;
 EnumStatusCode status = getLocalizedText (pValueStruct, returnValue);
 { if (!(StatusCode::isSUCCEEDED (status))) __builtin_abort (); } (void)0;
 return returnValue;
}

EnumStatusCode
LocalizedTextSet (LocalizedTextStruct* pTarget, LocalizedTextStruct* pSource)
{
 __builtin_strcpy (pTarget->getT (), pSource->getT ());
 return StatusCode::SUCCESS;
}
