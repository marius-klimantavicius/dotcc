/* PL/pgSQL's plpgsql.h forward-declares union YYSTYPE for its scanner API. */
union YYSTYPE;
int scanner_value(union YYSTYPE *value);
union YYSTYPE { int integer; };
int scanner_value(union YYSTYPE *value) { return value->integer; }
int main(void)
{
    union YYSTYPE value;
    value.integer = 42;
    return scanner_value(&value) == 42 ? 0 : 1;
}
