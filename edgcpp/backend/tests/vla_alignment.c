/* Explicit alignment uses the lowered pointer slot's alignment metadata. */
int vla_alignment(int n) {
 _Alignas(64) int a[n];
 _Alignas(256) unsigned char b[n+3];
 _Alignas(4096) long long c[n+1];
 if((unsigned long)a%64 || (unsigned long)b%256 || (unsigned long)c%4096)return 1;
 a[n-1]=19;b[n+2]=37;c[n]=0x102030405060708LL;
 return a[n-1]!=19 || b[n+2]!=37 || c[n]!=0x102030405060708LL;
}
#ifdef NATIVE_ORACLE
int main(void){return vla_alignment(31);}
#endif
