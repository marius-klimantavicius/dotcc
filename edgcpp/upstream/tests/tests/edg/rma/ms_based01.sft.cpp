//options_all:-r -x -tused
//options: --microsoft_version=1400 -n;cp

char c[] = { "123456789"};
char *pc = c;
char __based(pc) *bpc = (char __based(pc) *)(4*(sizeof(char)));
char __based (pc) *&rbpc = bpc;

main()
{
  /* set value of pointer through reference */
  rbpc = (char __based(pc) *)(3*(sizeof(char)));
  /* set value of pointer directly */
  bpc = (char __based(pc) *)(4*(sizeof(char)));
}

