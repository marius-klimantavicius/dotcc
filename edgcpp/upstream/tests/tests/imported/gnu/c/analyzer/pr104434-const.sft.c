//type: fp
//options: 
# 0 "./analyzer/pr104434-const.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr104434-const.c"


# 1 "./analyzer/pr104434.h" 1
# 26 "./analyzer/pr104434.h"
typedef long unsigned int size_t;

extern void *malloc (size_t __size)
  __attribute__ ((__nothrow__ , __leaf__))
  __attribute__ ((__malloc__))
  __attribute__ ((__alloc_size__ (1)))
  __attribute__ ((__warn_unused_result__));
extern void free (void *__ptr)
  __attribute__ ((__nothrow__ , __leaf__));
# 84 "./analyzer/pr104434.h"
typedef float _Complex lapack_complex_float;

void LAPACKE_xerbla( const char *name, int info );

void LAPACKE_cgb_trans( int matrix_layout, int m, int n,
                        int kl, int ku,
                        const lapack_complex_float *in, int ldin,
                        lapack_complex_float *out, int ldout );
void LAPACKE_cge_trans( int matrix_layout, int m, int n,
                        const lapack_complex_float* in, int ldin,
                        lapack_complex_float* out, int ldout );


void cgbbrd_(
    char const* vect,
    int const* m, int const* n, int const* ncc, int const* kl, int const* ku,
    lapack_complex_float* AB, int const* ldab,
    float* D,
    float* E,
    lapack_complex_float* Q, int const* ldq,
    lapack_complex_float* PT, int const* ldpt,
    lapack_complex_float* C, int const* ldc,
    lapack_complex_float* work,
    float* rwork,
    int* info );
# 4 "./analyzer/pr104434-const.c" 2


int LAPACKE_lsame( char ca, char cb ) __attribute__((const));
# 45 "./analyzer/pr104434-const.c"
int LAPACKE_cgbbrd_work( int matrix_layout, char vect, int m,
                                int n, int ncc, int kl,
                                int ku, lapack_complex_float* ab,
                                int ldab, float* d, float* e,
                                lapack_complex_float* q, int ldq,
                                lapack_complex_float* pt, int ldpt,
                                lapack_complex_float* c, int ldc,
                                lapack_complex_float* work, float* rwork )
{
    int info = 0;
    if( matrix_layout == 102 ) {

        cgbbrd_( &vect, &m, &n, &ncc, &kl, &ku, ab, &ldab, d, e, q, &ldq,
                       pt, &ldpt, c, &ldc, work, rwork, &info );
        if( info < 0 ) {
            info = info - 1;
        }
    } else if( matrix_layout == 101 ) {
        int ldab_t = (((1) > (kl+ku+1)) ? (1) : (kl+ku+1));
        int ldc_t = (((1) > (m)) ? (1) : (m));
        int ldpt_t = (((1) > (n)) ? (1) : (n));
        int ldq_t = (((1) > (m)) ? (1) : (m));
        lapack_complex_float* ab_t = ((void *)0);
        lapack_complex_float* q_t = ((void *)0);
        lapack_complex_float* pt_t = ((void *)0);
        lapack_complex_float* c_t = ((void *)0);

        if( ldab < n ) {
            info = -9;
            LAPACKE_xerbla( "LAPACKE_cgbbrd_work", info );
            return info;
        }
        if( ldc < ncc ) {
            info = -17;
            LAPACKE_xerbla( "LAPACKE_cgbbrd_work", info );
            return info;
        }
        if( ldpt < n ) {
            info = -15;
            LAPACKE_xerbla( "LAPACKE_cgbbrd_work", info );
            return info;
        }
        if( ldq < m ) {
            info = -13;
            LAPACKE_xerbla( "LAPACKE_cgbbrd_work", info );
            return info;
        }

        ab_t = (lapack_complex_float*)
            malloc( sizeof(lapack_complex_float) * ldab_t * (((1) > (n)) ? (1) : (n)) );
        if( ab_t == ((void *)0) ) {
            info = -1011;
            goto exit_level_0;
        }
        if( LAPACKE_lsame( vect, 'b' ) || LAPACKE_lsame( vect, 'q' ) ) {
            q_t = (lapack_complex_float*)
                malloc( sizeof(lapack_complex_float) * ldq_t * (((1) > (m)) ? (1) : (m)) )
                                                  ;
            if( q_t == ((void *)0) ) {
                info = -1011;
                goto exit_level_1;
            }
        }
        if( LAPACKE_lsame( vect, 'b' ) || LAPACKE_lsame( vect, 'p' ) ) {
            pt_t = (lapack_complex_float*)
                malloc( sizeof(lapack_complex_float) * ldpt_t * (((1) > (n)) ? (1) : (n)) )
                                                   ;
            if( pt_t == ((void *)0) ) {
                info = -1011;
                goto exit_level_2;
            }
        }
        if( ncc != 0 ) {
            c_t = (lapack_complex_float*)
                malloc( sizeof(lapack_complex_float) * ldc_t * (((1) > (ncc)) ? (1) : (ncc)) )
                                                    ;
            if( c_t == ((void *)0) ) {
                info = -1011;
                goto exit_level_3;
            }
        }

        LAPACKE_cgb_trans( matrix_layout, m, n, kl, ku, ab, ldab, ab_t, ldab_t );
        if( ncc != 0 ) {
            LAPACKE_cge_trans( matrix_layout, m, ncc, c, ldc, c_t, ldc_t );
        }

        cgbbrd_( &vect, &m, &n, &ncc, &kl, &ku, ab_t, &ldab_t, d, e, q_t,
                       &ldq_t, pt_t, &ldpt_t, c_t, &ldc_t, work, rwork, &info );
        if( info < 0 ) {
            info = info - 1;
        }

        LAPACKE_cgb_trans( 102, m, n, kl, ku, ab_t, ldab_t, ab,
                           ldab );
        if( LAPACKE_lsame( vect, 'b' ) || LAPACKE_lsame( vect, 'q' ) ) {
            LAPACKE_cge_trans( 102, m, m, q_t, ldq_t, q, ldq );
        }
        if( LAPACKE_lsame( vect, 'b' ) || LAPACKE_lsame( vect, 'p' ) ) {
            LAPACKE_cge_trans( 102, n, n, pt_t, ldpt_t, pt, ldpt );
        }
        if( ncc != 0 ) {
            LAPACKE_cge_trans( 102, m, ncc, c_t, ldc_t, c, ldc );
        }

        if( ncc != 0 ) {
            free( c_t );
        }
exit_level_3:
        if( LAPACKE_lsame( vect, 'b' ) || LAPACKE_lsame( vect, 'p' ) ) {
            free( pt_t );
        }
exit_level_2:
        if( LAPACKE_lsame( vect, 'b' ) || LAPACKE_lsame( vect, 'q' ) ) {
            free( q_t );
        }
exit_level_1:
        free( ab_t );
exit_level_0:
        if( info == -1011 ) {
            LAPACKE_xerbla( "LAPACKE_cgbbrd_work", info );
        }
    } else {
        info = -1;
        LAPACKE_xerbla( "LAPACKE_cgbbrd_work", info );
    }
    return info;

}
