/* Synthetic C99 VLA dimensions, pointer strides, scopes and recursive storage. */
static int dimensions;
static int dimension(int value) {++dimensions;return value;}
static int recursive(int n) {
 int values[n+1];
 for(int i=0;i<=n;++i)values[i]=n*100+i;
 int result=n?recursive(n-1):0;
 for(int i=0;i<=n;++i)if(values[i]!=n*100+i)return 1;
 return result;
}
static int matrix(int rows,int columns) {
 int a[dimension(rows)][dimension(columns)];
 int (*p)[columns]=a;
 unsigned long bytes=sizeof(a);
 for(int i=0;i<rows;++i)for(int j=0;j<columns;++j)a[i][j]=i*17+j;
 if((unsigned long)((char*)(p+1)-(char*)p)!=sizeof(int)*(unsigned long)columns)return 1;
 int old_rows=rows;rows=1;
 if(sizeof(a)!=bytes || bytes!=sizeof(int)*(unsigned long)old_rows*(unsigned long)columns)return 2;
 for(int i=0;i<old_rows;++i)for(int j=0;j<columns;++j)if(p[i][j]!=i*17+j)return 3;
 typedef int Row[dimension(columns)];
 Row next[2];next[1][columns-1]=719;
 if(sizeof(next)!=2UL*sizeof(int)*(unsigned long)columns || next[1][columns-1]!=719)return 4;
 return 0;
}
static int scopes(int n) {
 int outer[n];outer[0]=91;
 for(int i=0;i<50;++i) {
  int inner[n+i];inner[0]=i;
  if(i%3==0)continue;
  if(i==44)break;
  if(inner[0]!=i)return 1;
 }
 {int skipped[n+7];skipped[0]=19;if(skipped[0]==19)goto outside;return 2;}
outside:
 return outer[0]!=91;
}
int main(void) {
 dimensions=0;
 if(matrix(7,11) || dimensions!=3)return 1;
 if(scopes(5) || recursive(12))return 2;
 int zero=0;int empty[zero];if(sizeof(empty)!=0)return 3;
 int n=31;long double aligned[n];aligned[30]=1.25L;
 if((unsigned long)aligned%16 || aligned[30]!=1.25L)return 4;
 return 0;
}
