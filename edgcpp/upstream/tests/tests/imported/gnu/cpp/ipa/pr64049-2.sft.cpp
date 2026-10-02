//type: rp
//options: 
# 0 "./ipa/pr64049-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ipa/pr64049-2.C"



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
# 5 "./ipa/pr64049-2.C" 2

int
main ()
{
 ValueStruct v;
 v.arrayType = 0;
 v.dataType = 0;
 v.value.LocalizedText = new LocalizedTextStruct ("Localized Text");
 LocalizedText t = ValueHelper::getLocalizedText (&v);
 if (__builtin_strcmp (t.getInternHandle ()->getT (), "Localized Text"))
  __builtin_abort ();
 return 0;
}

LocalizedTextStruct*
LocalizedText::getInternHandle ()
{
 return &t;
}
