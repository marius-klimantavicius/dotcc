//type: rp
//options: --c23
# 0 "./atomic/stdatomic-bitint-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./atomic/stdatomic-bitint-2.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdatomic.h" 1 3 4
# 29 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdatomic.h" 3 4

# 29 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdatomic.h" 3 4
typedef enum
  {
    memory_order_relaxed = 0,
    memory_order_consume = 1,
    memory_order_acquire = 2,
    memory_order_release = 3,
    memory_order_acq_rel = 4,
    memory_order_seq_cst = 5
  } memory_order;


typedef _Atomic _Bool atomic_bool;
typedef _Atomic char atomic_char;
typedef _Atomic signed char atomic_schar;
typedef _Atomic unsigned char atomic_uchar;
typedef _Atomic short atomic_short;
typedef _Atomic unsigned short atomic_ushort;
typedef _Atomic int atomic_int;
typedef _Atomic unsigned int atomic_uint;
typedef _Atomic long atomic_long;
typedef _Atomic unsigned long atomic_ulong;
typedef _Atomic long long atomic_llong;
typedef _Atomic unsigned long long atomic_ullong;

typedef _Atomic unsigned char atomic_char8_t;

typedef _Atomic short unsigned int atomic_char16_t;
typedef _Atomic unsigned int atomic_char32_t;
typedef _Atomic int atomic_wchar_t;
typedef _Atomic signed char atomic_int_least8_t;
typedef _Atomic unsigned char atomic_uint_least8_t;
typedef _Atomic short int atomic_int_least16_t;
typedef _Atomic short unsigned int atomic_uint_least16_t;
typedef _Atomic int atomic_int_least32_t;
typedef _Atomic unsigned int atomic_uint_least32_t;
typedef _Atomic long int atomic_int_least64_t;
typedef _Atomic long unsigned int atomic_uint_least64_t;
typedef _Atomic signed char atomic_int_fast8_t;
typedef _Atomic unsigned char atomic_uint_fast8_t;
typedef _Atomic long int atomic_int_fast16_t;
typedef _Atomic long unsigned int atomic_uint_fast16_t;
typedef _Atomic long int atomic_int_fast32_t;
typedef _Atomic long unsigned int atomic_uint_fast32_t;
typedef _Atomic long int atomic_int_fast64_t;
typedef _Atomic long unsigned int atomic_uint_fast64_t;
typedef _Atomic long int atomic_intptr_t;
typedef _Atomic long unsigned int atomic_uintptr_t;
typedef _Atomic long unsigned int atomic_size_t;
typedef _Atomic long int atomic_ptrdiff_t;
typedef _Atomic long int atomic_intmax_t;
typedef _Atomic long unsigned int atomic_uintmax_t;
# 97 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdatomic.h" 3 4
extern void atomic_thread_fence (memory_order);

extern void atomic_signal_fence (memory_order);
# 226 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdatomic.h" 3 4
typedef _Atomic struct
{

  _Bool __val;



} atomic_flag;




extern _Bool atomic_flag_test_and_set (volatile atomic_flag *);


extern _Bool atomic_flag_test_and_set_explicit (volatile atomic_flag *,
      memory_order);



extern void atomic_flag_clear (volatile atomic_flag *);

extern void atomic_flag_clear_explicit (volatile atomic_flag *, memory_order);
# 6 "./atomic/stdatomic-bitint-2.c" 2


# 7 "./atomic/stdatomic-bitint-2.c"
extern void abort (void);


_Atomic _BitInt(575) v;
_BitInt(575) count, res;
const _BitInt(575) init = ~(_BitInt(575)) 0wb;

void
test_fetch_add ()
{
  
# 17 "./atomic/stdatomic-bitint-2.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 17 "./atomic/stdatomic-bitint-2.c"
 &v
# 17 "./atomic/stdatomic-bitint-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 17 "./atomic/stdatomic-bitint-2.c"
 59465222573183779324781274162178653782927579944977967117312772499849358939735575735090252965265846823956223131844773977101864574011188590603103408821590130462520809924774161wb
# 17 "./atomic/stdatomic-bitint-2.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (0)); })
# 17 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                                  ;
  count = 21849324526703540909725517290562575722142104889154621021004438836543599493803029317660194646869455042293514095831327249339063542203879269024249546998746919066599380031180974wb;

  if (
# 20 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_add ((
# 20 "./atomic/stdatomic-bitint-2.c"
     &v
# 20 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 20 "./atomic/stdatomic-bitint-2.c"
     count
# 20 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 20 "./atomic/stdatomic-bitint-2.c"
     memory_order_relaxed
# 20 "./atomic/stdatomic-bitint-2.c" 3 4
     ))
      
# 21 "./atomic/stdatomic-bitint-2.c"
     != 59465222573183779324781274162178653782927579944977967117312772499849358939735575735090252965265846823956223131844773977101864574011188590603103408821590130462520809924774161wb)
    abort ();

  if (
# 24 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_add ((
# 24 "./atomic/stdatomic-bitint-2.c"
     &v
# 24 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 24 "./atomic/stdatomic-bitint-2.c"
     -48324598397571087754171506195219221853271763472221035086980364394554212400270766919963305485900701475788840363160834710492683133292253640988518950921616217692973141455340373wb
# 24 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 24 "./atomic/stdatomic-bitint-2.c"
     memory_order_consume
# 24 "./atomic/stdatomic-bitint-2.c" 3 4
     ))

      
# 26 "./atomic/stdatomic-bitint-2.c"
     != -42350653636664946795744469057082365512495989716473331818714316710055654119727328532407752918486220628549098485331968443234754400938307745356420121730609534429183196118394433wb)
    abort ();

  if (
# 29 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_add ((
# 29 "./atomic/stdatomic-bitint-2.c"
     &v
# 29 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 29 "./atomic/stdatomic-bitint-2.c"
     count
# 29 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 29 "./atomic/stdatomic-bitint-2.c"
     memory_order_acquire
# 29 "./atomic/stdatomic-bitint-2.c" 3 4
     ))
      
# 30 "./atomic/stdatomic-bitint-2.c"
     != 32989948702316232480335285257522007651797921361911553051336846941838746033267838132787142126234600390460896864515266515948244982922814218638834004898720831836147048500614762wb)
    abort ();

  if (
# 33 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_add ((
# 33 "./atomic/stdatomic-bitint-2.c"
     &v
# 33 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 33 "./atomic/stdatomic-bitint-2.c"
     6958312589905983216078981134518695082538588883298046610645489916662738759809700053844918067866491080683973037493524864531300139437943661326918145212620326291059845453630984wb
# 33 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 33 "./atomic/stdatomic-bitint-2.c"
     memory_order_release
# 33 "./atomic/stdatomic-bitint-2.c" 3 4
     ))

      
# 35 "./atomic/stdatomic-bitint-2.c"
     != 54839273229019773390060802548084583373940026251066174072341285778382345527070867450447336773104055432754410960346593765287308525126693487663083551897467750902746428531795736wb)
    abort ();

  if (
# 38 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_add ((
# 38 "./atomic/stdatomic-bitint-2.c"
     &v
# 38 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 38 "./atomic/stdatomic-bitint-2.c"
     count
# 38 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 38 "./atomic/stdatomic-bitint-2.c"
     memory_order_acq_rel
# 38 "./atomic/stdatomic-bitint-2.c" 3 4
     ))
      
# 39 "./atomic/stdatomic-bitint-2.c"
     != 61797585818925756606139783682603278456478615134364220682986775695045084286880567504292254840970546513438383997840118629818608664564637148990001697110088077193806273985426720wb)
    abort ();

  if (
# 42 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_add ((
# 42 "./atomic/stdatomic-bitint-2.c"
     &v
# 42 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 42 "./atomic/stdatomic-bitint-2.c"
     -40070085597220253007443375012839311464160438432662764715171645999337062215395237625623334979615599652975898123428005079853660189572782012739700306191422569739682093304494995wb
# 42 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 42 "./atomic/stdatomic-bitint-2.c"
     memory_order_seq_cst
# 42 "./atomic/stdatomic-bitint-2.c" 3 4
     ))

      
# 44 "./atomic/stdatomic-bitint-2.c"
     != -40018290390922969514385959536657740838944954527087078253040313514859928772582336763205751042781520939066937619336623790518010310384859186969521833442111587697897732057741874wb)
    abort ();

  if (
# 47 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_add ((
# 47 "./atomic/stdatomic-bitint-2.c"
     &v
# 47 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 47 "./atomic/stdatomic-bitint-2.c"
     7196090098608011755205055682070541953902689411792805630101863619558545582022760829169235622333653743217332966939875542415224317593660770300818700993340012969018217868600175wb
# 47 "./atomic/stdatomic-bitint-2.c" 3 4
     ), 5)
      
# 48 "./atomic/stdatomic-bitint-2.c"
     != 43576824748409044508421925960326542714460281590856076988819568532251621565288359196329114508224401902755999970243440799304012017195734405274550937917412426520723560712112699wb)
    abort ();

  if (
# 51 "./atomic/stdatomic-bitint-2.c" 3 4
     __extension__ ({ __auto_type __atomic_load_ptr = (
# 51 "./atomic/stdatomic-bitint-2.c"
     &v
# 51 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 51 "./atomic/stdatomic-bitint-2.c"
                      != 50772914847017056263626981642397084668362971002648882618921432151810167147311120025498350130558055645973332937183316341719236334789395175575369638910752439489741778580712874wb)
    abort ();
}

void
test_fetch_sub ()
{
  
# 58 "./atomic/stdatomic-bitint-2.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 58 "./atomic/stdatomic-bitint-2.c"
 &v
# 58 "./atomic/stdatomic-bitint-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 58 "./atomic/stdatomic-bitint-2.c"
 -24875491091433158113922205635657739252057730543056417031972993454853665637813018617125143194799847437263037065304695383952916979113678037118532311779822859742705294727822376wb
# 58 "./atomic/stdatomic-bitint-2.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (
# 58 "./atomic/stdatomic-bitint-2.c"
 memory_order_release
# 58 "./atomic/stdatomic-bitint-2.c" 3 4
 )); })
                         
# 59 "./atomic/stdatomic-bitint-2.c"
                        ;
  count = 6813702694653136917886567607003795360731391695538381923575500076220746287948057449348609672603971831069550813465757196210073636056135824219022209113495061317682354090030599wb;

  if (
# 62 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_sub ((
# 62 "./atomic/stdatomic-bitint-2.c"
     &v
# 62 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 62 "./atomic/stdatomic-bitint-2.c"
     count
# 62 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 62 "./atomic/stdatomic-bitint-2.c"
     memory_order_relaxed
# 62 "./atomic/stdatomic-bitint-2.c" 3 4
     ))
      
# 63 "./atomic/stdatomic-bitint-2.c"
     != -24875491091433158113922205635657739252057730543056417031972993454853665637813018617125143194799847437263037065304695383952916979113678037118532311779822859742705294727822376wb)
    abort ();

  if (
# 66 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_sub ((
# 66 "./atomic/stdatomic-bitint-2.c"
     &v
# 66 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 66 "./atomic/stdatomic-bitint-2.c"
     8502925336737158389204618905966437550402192530184998378452912098895816391400915282714213521081597588768568861040471642522181200007537708168290327879425612502090295717806034wb
# 66 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 66 "./atomic/stdatomic-bitint-2.c"
     memory_order_consume
# 66 "./atomic/stdatomic-bitint-2.c" 3 4
     ))

      
# 68 "./atomic/stdatomic-bitint-2.c"
     != -31689193786086295031808773242661534612789122238594798955548493531074411925761076066473752867403819268332587878770452580162990615169813861337554520893317921060387648817852975wb)
    abort ();

  if (
# 71 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_sub ((
# 71 "./atomic/stdatomic-bitint-2.c"
     &v
# 71 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 71 "./atomic/stdatomic-bitint-2.c"
     count
# 71 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 71 "./atomic/stdatomic-bitint-2.c"
     memory_order_acquire
# 71 "./atomic/stdatomic-bitint-2.c" 3 4
     ))
      
# 72 "./atomic/stdatomic-bitint-2.c"
     != -40192119122823453421013392148627972163191314768779797334001405629970228317161991349187966388485416857101156739810924222685171815177351569505844848772743533562477944535659009wb)
    abort ();

  if (
# 75 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_sub ((
# 75 "./atomic/stdatomic-bitint-2.c"
     &v
# 75 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 75 "./atomic/stdatomic-bitint-2.c"
     -14654459030451169455889477114198307761108411115104251375487725308584250511764953921137486025925617622862411621191578047606524662379041353882420330350079004108508600686109553wb
# 75 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 75 "./atomic/stdatomic-bitint-2.c"
     memory_order_release
# 75 "./atomic/stdatomic-bitint-2.c" 3 4
     ))

      
# 77 "./atomic/stdatomic-bitint-2.c"
     != -47005821817476590338899959755631767523922706464318179257576905706190974605110048798536576061089388688170707553276681418895245451233487393724867057886238594880160298625689608wb)
    abort ();

  if (
# 80 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_sub ((
# 80 "./atomic/stdatomic-bitint-2.c"
     &v
# 80 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 80 "./atomic/stdatomic-bitint-2.c"
     count
# 80 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 80 "./atomic/stdatomic-bitint-2.c"
     memory_order_acq_rel
# 80 "./atomic/stdatomic-bitint-2.c" 3 4
     ))
      
# 81 "./atomic/stdatomic-bitint-2.c"
     != -32351362787025420883010482641433459762814295349213927882089180397606724093345094877399090035163771065308295932085103371288720788854446039842446727536159590771651697939580055wb)
    abort ();

  if (
# 84 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_sub ((
# 84 "./atomic/stdatomic-bitint-2.c"
     &v
# 84 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 84 "./atomic/stdatomic-bitint-2.c"
     22886836433700729148520267039236396292049781709943309196356241086882444816631218673962641435859436811686812355905147988710440709035301495069683757516099446883747069843672565wb
# 84 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 84 "./atomic/stdatomic-bitint-2.c"
     memory_order_seq_cst
# 84 "./atomic/stdatomic-bitint-2.c" 3 4
     ))

      
# 86 "./atomic/stdatomic-bitint-2.c"
     != -39165065481678557800897050248437255123545687044752309805664680473827470381293152326747699707767742896377846745550860567498794424910581864061468936649654652089334052029610654wb)
    abort ();

  if (
# 89 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_sub ((
# 89 "./atomic/stdatomic-bitint-2.c"
     &v
# 89 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 89 "./atomic/stdatomic-bitint-2.c"
     49406467535448986167225179068150076098092817730200780462287871488076089290458383699602612866098591282427581289588523598202726797161439183859560135056424525356253246480887928wb
# 89 "./atomic/stdatomic-bitint-2.c" 3 4
     ), 5)
      
# 90 "./atomic/stdatomic-bitint-2.c"
     != 61613298821172980080833943222149943601970205795910300955010606485738697355341562584447859386994342786734176611552061113466447383207492245852620383385192484985222264201066349wb)
    abort ();

  if (
# 93 "./atomic/stdatomic-bitint-2.c" 3 4
     __extension__ ({ __auto_type __atomic_load_ptr = (
# 93 "./atomic/stdatomic-bitint-2.c"
     &v
# 93 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (
# 93 "./atomic/stdatomic-bitint-2.c"
     memory_order_acquire
# 93 "./atomic/stdatomic-bitint-2.c" 3 4
     )); __atomic_load_tmp; })
      
# 94 "./atomic/stdatomic-bitint-2.c"
     != 12206831285723993913608764153999867503877388065709520492722734997662608064883178884845246520895751504306595321963537515263720586046053061993060248328767959628969017720178421wb)
    abort ();
}

void
test_fetch_and ()
{
  
# 101 "./atomic/stdatomic-bitint-2.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 101 "./atomic/stdatomic-bitint-2.c"
 &v
# 101 "./atomic/stdatomic-bitint-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 101 "./atomic/stdatomic-bitint-2.c"
 init
# 101 "./atomic/stdatomic-bitint-2.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (5)); })
# 101 "./atomic/stdatomic-bitint-2.c"
                        ;

  if (
# 103 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_and ((
# 103 "./atomic/stdatomic-bitint-2.c"
     &v
# 103 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 103 "./atomic/stdatomic-bitint-2.c"
     0
# 103 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 103 "./atomic/stdatomic-bitint-2.c"
     memory_order_relaxed
# 103 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 103 "./atomic/stdatomic-bitint-2.c"
                                                             != init)
    abort ();

  if (
# 106 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_and ((
# 106 "./atomic/stdatomic-bitint-2.c"
     &v
# 106 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 106 "./atomic/stdatomic-bitint-2.c"
     init
# 106 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 106 "./atomic/stdatomic-bitint-2.c"
     memory_order_consume
# 106 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 106 "./atomic/stdatomic-bitint-2.c"
                                                                != 0)
    abort ();

  if (
# 109 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_and ((
# 109 "./atomic/stdatomic-bitint-2.c"
     &v
# 109 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 109 "./atomic/stdatomic-bitint-2.c"
     0
# 109 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 109 "./atomic/stdatomic-bitint-2.c"
     memory_order_acquire
# 109 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 109 "./atomic/stdatomic-bitint-2.c"
                                                             != 0)
    abort ();

  v = ~v;
  if (
# 113 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_and ((
# 113 "./atomic/stdatomic-bitint-2.c"
     &v
# 113 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 113 "./atomic/stdatomic-bitint-2.c"
     init
# 113 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 113 "./atomic/stdatomic-bitint-2.c"
     memory_order_release
# 113 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 113 "./atomic/stdatomic-bitint-2.c"
                                                                != init)
    abort ();

  if (
# 116 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_and ((
# 116 "./atomic/stdatomic-bitint-2.c"
     &v
# 116 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 116 "./atomic/stdatomic-bitint-2.c"
     0
# 116 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 116 "./atomic/stdatomic-bitint-2.c"
     memory_order_acq_rel
# 116 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 116 "./atomic/stdatomic-bitint-2.c"
                                                             != init)
    abort ();

  if (
# 119 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_and ((
# 119 "./atomic/stdatomic-bitint-2.c"
     &v
# 119 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 119 "./atomic/stdatomic-bitint-2.c"
     0
# 119 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 119 "./atomic/stdatomic-bitint-2.c"
     memory_order_seq_cst
# 119 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 119 "./atomic/stdatomic-bitint-2.c"
                                                             != 0)
    abort ();

  if (
# 122 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_and ((
# 122 "./atomic/stdatomic-bitint-2.c"
     &v
# 122 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 122 "./atomic/stdatomic-bitint-2.c"
     0
# 122 "./atomic/stdatomic-bitint-2.c" 3 4
     ), 5) 
# 122 "./atomic/stdatomic-bitint-2.c"
                              != 0)
    abort ();
}

void
test_fetch_xor ()
{
  v = init;
  count = 0;

  if (
# 132 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_xor ((
# 132 "./atomic/stdatomic-bitint-2.c"
     &v
# 132 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 132 "./atomic/stdatomic-bitint-2.c"
     count
# 132 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 132 "./atomic/stdatomic-bitint-2.c"
     memory_order_relaxed
# 132 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 132 "./atomic/stdatomic-bitint-2.c"
                                                                 != init)
    abort ();

  if (
# 135 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_xor ((
# 135 "./atomic/stdatomic-bitint-2.c"
     &v
# 135 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 135 "./atomic/stdatomic-bitint-2.c"
     ~count
# 135 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 135 "./atomic/stdatomic-bitint-2.c"
     memory_order_consume
# 135 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 135 "./atomic/stdatomic-bitint-2.c"
                                                                  != init)
    abort ();

  if (
# 138 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_xor ((
# 138 "./atomic/stdatomic-bitint-2.c"
     &v
# 138 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 138 "./atomic/stdatomic-bitint-2.c"
     0
# 138 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 138 "./atomic/stdatomic-bitint-2.c"
     memory_order_acquire
# 138 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 138 "./atomic/stdatomic-bitint-2.c"
                                                             != 0)
    abort ();

  if (
# 141 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_xor ((
# 141 "./atomic/stdatomic-bitint-2.c"
     &v
# 141 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 141 "./atomic/stdatomic-bitint-2.c"
     ~count
# 141 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 141 "./atomic/stdatomic-bitint-2.c"
     memory_order_release
# 141 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 141 "./atomic/stdatomic-bitint-2.c"
                                                                  != 0)
    abort ();

  if (
# 144 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_xor ((
# 144 "./atomic/stdatomic-bitint-2.c"
     &v
# 144 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 144 "./atomic/stdatomic-bitint-2.c"
     0
# 144 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 144 "./atomic/stdatomic-bitint-2.c"
     memory_order_acq_rel
# 144 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 144 "./atomic/stdatomic-bitint-2.c"
                                                             != init)
    abort ();

  if (
# 147 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_xor ((
# 147 "./atomic/stdatomic-bitint-2.c"
     &v
# 147 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 147 "./atomic/stdatomic-bitint-2.c"
     ~count
# 147 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 147 "./atomic/stdatomic-bitint-2.c"
     memory_order_seq_cst
# 147 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 147 "./atomic/stdatomic-bitint-2.c"
                                                                  != init)
    abort ();

  if (
# 150 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_xor ((
# 150 "./atomic/stdatomic-bitint-2.c"
     &v
# 150 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 150 "./atomic/stdatomic-bitint-2.c"
     ~count
# 150 "./atomic/stdatomic-bitint-2.c" 3 4
     ), 5) 
# 150 "./atomic/stdatomic-bitint-2.c"
                                   != 0)
    abort ();
}

void
test_fetch_or ()
{
  v = 0wb;
  count = 28269553036454149273332760011886696253239742350009903329945699220681916416wb;

  if (
# 160 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_or ((
# 160 "./atomic/stdatomic-bitint-2.c"
     &v
# 160 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 160 "./atomic/stdatomic-bitint-2.c"
     count
# 160 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 160 "./atomic/stdatomic-bitint-2.c"
     memory_order_relaxed
# 160 "./atomic/stdatomic-bitint-2.c" 3 4
     )) 
# 160 "./atomic/stdatomic-bitint-2.c"
                                                                != 0wb)
    abort ();

  count *= 2;
  if (
# 164 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_or ((
# 164 "./atomic/stdatomic-bitint-2.c"
     &v
# 164 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 164 "./atomic/stdatomic-bitint-2.c"
     56539106072908298546665520023773392506479484700019806659891398441363832832wb
# 164 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 164 "./atomic/stdatomic-bitint-2.c"
     memory_order_consume
# 164 "./atomic/stdatomic-bitint-2.c" 3 4
     ))

      
# 166 "./atomic/stdatomic-bitint-2.c"
     != 28269553036454149273332760011886696253239742350009903329945699220681916416wb)
    abort ();

  count *= 2;
  if (
# 170 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_or ((
# 170 "./atomic/stdatomic-bitint-2.c"
     &v
# 170 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 170 "./atomic/stdatomic-bitint-2.c"
     count
# 170 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 170 "./atomic/stdatomic-bitint-2.c"
     memory_order_acquire
# 170 "./atomic/stdatomic-bitint-2.c" 3 4
     ))
      
# 171 "./atomic/stdatomic-bitint-2.c"
     != 84808659109362447819998280035660088759719227050029709989837097662045749248wb)
    abort ();

  count *= 2;
  if (
# 175 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_or ((
# 175 "./atomic/stdatomic-bitint-2.c"
     &v
# 175 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 175 "./atomic/stdatomic-bitint-2.c"
     226156424291633194186662080095093570025917938800079226639565593765455331328wb
# 175 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 175 "./atomic/stdatomic-bitint-2.c"
     memory_order_release
# 175 "./atomic/stdatomic-bitint-2.c" 3 4
     ))

      
# 177 "./atomic/stdatomic-bitint-2.c"
     != 197886871255179044913329320083206873772678196450069323309619894544773414912wb)
    abort ();

  count *= 2;
  if (
# 181 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_or ((
# 181 "./atomic/stdatomic-bitint-2.c"
     &v
# 181 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 181 "./atomic/stdatomic-bitint-2.c"
     count
# 181 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 181 "./atomic/stdatomic-bitint-2.c"
     memory_order_acq_rel
# 181 "./atomic/stdatomic-bitint-2.c" 3 4
     ))
      
# 182 "./atomic/stdatomic-bitint-2.c"
     != 424043295546812239099991400178300443798596135250148549949185488310228746240wb)
    abort ();

  count *= 2;
  if (
# 186 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_or ((
# 186 "./atomic/stdatomic-bitint-2.c"
     &v
# 186 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 186 "./atomic/stdatomic-bitint-2.c"
     count
# 186 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 186 "./atomic/stdatomic-bitint-2.c"
     memory_order_seq_cst
# 186 "./atomic/stdatomic-bitint-2.c" 3 4
     ))
      
# 187 "./atomic/stdatomic-bitint-2.c"
     != 876356144130078627473315560368487583850432012850307003228316675841139408896wb)
    abort ();

  count *= 2;
  if (
# 191 "./atomic/stdatomic-bitint-2.c" 3 4
     __atomic_fetch_or ((
# 191 "./atomic/stdatomic-bitint-2.c"
     &v
# 191 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 191 "./atomic/stdatomic-bitint-2.c"
     count
# 191 "./atomic/stdatomic-bitint-2.c" 3 4
     ), 5) 
# 191 "./atomic/stdatomic-bitint-2.c"
                                 != 1780981841296611404219963880748861863954103768050623909786579050902960734208wb)
    abort ();
}




void
test_add ()
{
  v = 0;
  count = 1486842905609751694333995980623369134886360644481888406879896752089037065255179570086557965294767349553406889549100250674362935254187127254353474822610854717612397677wb;

  
# 204 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_add ((
# 204 "./atomic/stdatomic-bitint-2.c"
 &v
# 204 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 204 "./atomic/stdatomic-bitint-2.c"
 count
# 204 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 204 "./atomic/stdatomic-bitint-2.c"
                             ;
  if (v != 1486842905609751694333995980623369134886360644481888406879896752089037065255179570086557965294767349553406889549100250674362935254187127254353474822610854717612397677wb)
    abort ();

  
# 208 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_add ((
# 208 "./atomic/stdatomic-bitint-2.c"
 &v
# 208 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 208 "./atomic/stdatomic-bitint-2.c"
 count
# 208 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 208 "./atomic/stdatomic-bitint-2.c"
 memory_order_consume
# 208 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 208 "./atomic/stdatomic-bitint-2.c"
                                                            ;
  if (v != 2973685811219503388667991961246738269772721288963776813759793504178074130510359140173115930589534699106813779098200501348725870508374254508706949645221709435224795354wb)
    abort ();

  
# 212 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_add ((
# 212 "./atomic/stdatomic-bitint-2.c"
 &v
# 212 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 212 "./atomic/stdatomic-bitint-2.c"
 1486842905609751694333995980623369134886360644481888406879896752089037065255179570086557965294767349553406889549100250674362935254187127254353474822610854717612397677wb
# 212 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 212 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                                ;
  if (v != 4460528716829255083001987941870107404659081933445665220639690256267111195765538710259673895884302048660220668647300752023088805762561381763060424467832564152837193031wb)
    abort ();

  
# 216 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_add ((
# 216 "./atomic/stdatomic-bitint-2.c"
 &v
# 216 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 216 "./atomic/stdatomic-bitint-2.c"
 1486842905609751694333995980623369134886360644481888406879896752089037065255179570086557965294767349553406889549100250674362935254187127254353474822610854717612397677wb
# 216 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 216 "./atomic/stdatomic-bitint-2.c"
 memory_order_release
# 216 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
                             
# 217 "./atomic/stdatomic-bitint-2.c"
                            ;
  if (v != 5947371622439006777335983922493476539545442577927553627519587008356148261020718280346231861179069398213627558196401002697451741016748509017413899290443418870449590708wb)
    abort ();

  
# 221 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_add ((
# 221 "./atomic/stdatomic-bitint-2.c"
 &v
# 221 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 221 "./atomic/stdatomic-bitint-2.c"
 1486842905609751694333995980623369134886360644481888406879896752089037065255179570086557965294767349553406889549100250674362935254187127254353474822610854717612397677wb
# 221 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 221 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                                ;
  if (v != 7434214528048758471669979903116845674431803222409442034399483760445185326275897850432789826473836747767034447745501253371814676270935636271767374113054273588061988385wb)
    abort ();

  
# 225 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_add ((
# 225 "./atomic/stdatomic-bitint-2.c"
 &v
# 225 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 225 "./atomic/stdatomic-bitint-2.c"
 count
# 225 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 225 "./atomic/stdatomic-bitint-2.c"
 memory_order_seq_cst
# 225 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 225 "./atomic/stdatomic-bitint-2.c"
                                                            ;
  if (v != 8921057433658510166003975883740214809318163866891330441279380512534222391531077420519347791768604097320441337294601504046177611525122763526120848935665128305674386062wb)
    abort ();
}

void
test_sub ()
{
  v = res = 55339930658115792138308584702507715233391812567721958037499562620485942364609138719919541704955047331226014242859673113345829202990143617633801993282898070230404539826267403wb;
  count = 0;

  
# 236 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_sub ((
# 236 "./atomic/stdatomic-bitint-2.c"
 &v
# 236 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 236 "./atomic/stdatomic-bitint-2.c"
 count + 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb
# 236 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 236 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                               ;
  res -= 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb;
  if (v != res)
    abort ();

  
# 241 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_sub ((
# 241 "./atomic/stdatomic-bitint-2.c"
 &v
# 241 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 241 "./atomic/stdatomic-bitint-2.c"
 count + 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb
# 241 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 241 "./atomic/stdatomic-bitint-2.c"
 memory_order_consume
# 241 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 241 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                                                              ;
  res -= 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb;
  if (v != res)
    abort ();

  
# 246 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_sub ((
# 246 "./atomic/stdatomic-bitint-2.c"
 &v
# 246 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 246 "./atomic/stdatomic-bitint-2.c"
 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb
# 246 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 246 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                       ;
  res -= 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb;
  if (v != res)
    abort ();

  
# 251 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_sub ((
# 251 "./atomic/stdatomic-bitint-2.c"
 &v
# 251 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 251 "./atomic/stdatomic-bitint-2.c"
 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb
# 251 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 251 "./atomic/stdatomic-bitint-2.c"
 memory_order_release
# 251 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 251 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                                                      ;
  res -= 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb;
  if (v != res)
    abort ();

  
# 256 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_sub ((
# 256 "./atomic/stdatomic-bitint-2.c"
 &v
# 256 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 256 "./atomic/stdatomic-bitint-2.c"
 count + 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb
# 256 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 256 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                               ;
  res -= 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb;
  if (v != res)
    abort ();

  
# 261 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_sub ((
# 261 "./atomic/stdatomic-bitint-2.c"
 &v
# 261 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 261 "./atomic/stdatomic-bitint-2.c"
 count + 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb
# 261 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 261 "./atomic/stdatomic-bitint-2.c"
 memory_order_seq_cst
# 261 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 261 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                                                              ;
  res -= 2266681016524228072657464685355732111725088152428495977635446886262656759828446929716740827028716649170693025791717921736186782036315322643449879013364541057wb;
  if (v != res)
    abort ();
}

void
test_and ()
{
  v = init;

  
# 272 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_and ((
# 272 "./atomic/stdatomic-bitint-2.c"
 &v
# 272 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 272 "./atomic/stdatomic-bitint-2.c"
 0
# 272 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 272 "./atomic/stdatomic-bitint-2.c"
                         ;
  if (v != 0)
    abort ();

  v = init;
  
# 277 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_and ((
# 277 "./atomic/stdatomic-bitint-2.c"
 &v
# 277 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 277 "./atomic/stdatomic-bitint-2.c"
 init
# 277 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 277 "./atomic/stdatomic-bitint-2.c"
 memory_order_consume
# 277 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 277 "./atomic/stdatomic-bitint-2.c"
                                                           ;
  if (v != init)
    abort ();

  
# 281 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_and ((
# 281 "./atomic/stdatomic-bitint-2.c"
 &v
# 281 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 281 "./atomic/stdatomic-bitint-2.c"
 0
# 281 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 281 "./atomic/stdatomic-bitint-2.c"
                         ;
  if (v != 0)
    abort ();

  v = ~v;
  
# 286 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_and ((
# 286 "./atomic/stdatomic-bitint-2.c"
 &v
# 286 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 286 "./atomic/stdatomic-bitint-2.c"
 init
# 286 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 286 "./atomic/stdatomic-bitint-2.c"
 memory_order_release
# 286 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 286 "./atomic/stdatomic-bitint-2.c"
                                                           ;
  if (v != init)
    abort ();

  
# 290 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_and ((
# 290 "./atomic/stdatomic-bitint-2.c"
 &v
# 290 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 290 "./atomic/stdatomic-bitint-2.c"
 0
# 290 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 290 "./atomic/stdatomic-bitint-2.c"
                         ;
  if (v != 0)
    abort ();

  v = ~v;
  
# 295 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_and ((
# 295 "./atomic/stdatomic-bitint-2.c"
 &v
# 295 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 295 "./atomic/stdatomic-bitint-2.c"
 0
# 295 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 295 "./atomic/stdatomic-bitint-2.c"
 memory_order_seq_cst
# 295 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 295 "./atomic/stdatomic-bitint-2.c"
                                                        ;
  if (v != 0)
    abort ();
}

void
test_xor ()
{
  v = init;
  count = 0;

  
# 306 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_xor ((
# 306 "./atomic/stdatomic-bitint-2.c"
 &v
# 306 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 306 "./atomic/stdatomic-bitint-2.c"
 count
# 306 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 306 "./atomic/stdatomic-bitint-2.c"
                             ;
  if (v != init)
    abort ();

  
# 310 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_xor ((
# 310 "./atomic/stdatomic-bitint-2.c"
 &v
# 310 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 310 "./atomic/stdatomic-bitint-2.c"
 ~count
# 310 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 310 "./atomic/stdatomic-bitint-2.c"
 memory_order_consume
# 310 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 310 "./atomic/stdatomic-bitint-2.c"
                                                             ;
  if (v != 0)
    abort ();

  
# 314 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_xor ((
# 314 "./atomic/stdatomic-bitint-2.c"
 &v
# 314 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 314 "./atomic/stdatomic-bitint-2.c"
 0
# 314 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 314 "./atomic/stdatomic-bitint-2.c"
                         ;
  if (v != 0)
    abort ();

  
# 318 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_xor ((
# 318 "./atomic/stdatomic-bitint-2.c"
 &v
# 318 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 318 "./atomic/stdatomic-bitint-2.c"
 ~count
# 318 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 318 "./atomic/stdatomic-bitint-2.c"
 memory_order_release
# 318 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 318 "./atomic/stdatomic-bitint-2.c"
                                                             ;
  if (v != init)
    abort ();

  
# 322 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_xor ((
# 322 "./atomic/stdatomic-bitint-2.c"
 &v
# 322 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 322 "./atomic/stdatomic-bitint-2.c"
 0
# 322 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 322 "./atomic/stdatomic-bitint-2.c"
 memory_order_acq_rel
# 322 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 322 "./atomic/stdatomic-bitint-2.c"
                                                        ;
  if (v != init)
    abort ();

  
# 326 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_xor ((
# 326 "./atomic/stdatomic-bitint-2.c"
 &v
# 326 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 326 "./atomic/stdatomic-bitint-2.c"
 ~count
# 326 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 326 "./atomic/stdatomic-bitint-2.c"
                              ;
  if (v != 0)
    abort ();
}

void
test_or ()
{
  v = 0;
  count = 19342813113834066795298816wb;

  
# 337 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_or ((
# 337 "./atomic/stdatomic-bitint-2.c"
 &v
# 337 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 337 "./atomic/stdatomic-bitint-2.c"
 count
# 337 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 337 "./atomic/stdatomic-bitint-2.c"
                            ;
  if (v != 19342813113834066795298816wb)
    abort ();

  count *= 2;
  
# 342 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_or ((
# 342 "./atomic/stdatomic-bitint-2.c"
 &v
# 342 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 342 "./atomic/stdatomic-bitint-2.c"
 count
# 342 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 342 "./atomic/stdatomic-bitint-2.c"
 memory_order_consume
# 342 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 342 "./atomic/stdatomic-bitint-2.c"
                                                           ;
  if (v != 58028439341502200385896448wb)
    abort ();

  count *= 2;
  
# 347 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_or ((
# 347 "./atomic/stdatomic-bitint-2.c"
 &v
# 347 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 347 "./atomic/stdatomic-bitint-2.c"
 77371252455336267181195264wb
# 347 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 347 "./atomic/stdatomic-bitint-2.c"
                                                   ;
  if (v != 135399691796838467567091712wb)
    abort ();

  count *= 2;
  
# 352 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_or ((
# 352 "./atomic/stdatomic-bitint-2.c"
 &v
# 352 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 352 "./atomic/stdatomic-bitint-2.c"
 154742504910672534362390528wb
# 352 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 352 "./atomic/stdatomic-bitint-2.c"
 memory_order_release
# 352 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
                            
# 353 "./atomic/stdatomic-bitint-2.c"
                           ;
  if (v != 290142196707511001929482240wb)
    abort ();

  count *= 2;
  
# 358 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_or ((
# 358 "./atomic/stdatomic-bitint-2.c"
 &v
# 358 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 358 "./atomic/stdatomic-bitint-2.c"
 count
# 358 "./atomic/stdatomic-bitint-2.c" 3 4
 ), 5)
# 358 "./atomic/stdatomic-bitint-2.c"
                            ;
  if (v != 599627206528856070654263296wb)
    abort ();

  count *= 2;
  
# 363 "./atomic/stdatomic-bitint-2.c" 3 4
 __atomic_fetch_or ((
# 363 "./atomic/stdatomic-bitint-2.c"
 &v
# 363 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 363 "./atomic/stdatomic-bitint-2.c"
 count
# 363 "./atomic/stdatomic-bitint-2.c" 3 4
 ), (
# 363 "./atomic/stdatomic-bitint-2.c"
 memory_order_seq_cst
# 363 "./atomic/stdatomic-bitint-2.c" 3 4
 ))
# 363 "./atomic/stdatomic-bitint-2.c"
                                                           ;
  if (v != 1218597226171546208103825408wb)
    abort ();
}

void
test_exchange (void)
{
  
# 371 "./atomic/stdatomic-bitint-2.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 371 "./atomic/stdatomic-bitint-2.c"
 &v
# 371 "./atomic/stdatomic-bitint-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 371 "./atomic/stdatomic-bitint-2.c"
 -285679222948993342888321238830899996788334328454283006589115162661816862491992700267590986826514460282130796141107636816730207482542791159837024986802592659048696136633096wb
# 371 "./atomic/stdatomic-bitint-2.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (5)); })
# 371 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                                  ;
  if (
# 372 "./atomic/stdatomic-bitint-2.c" 3 4
     __extension__ ({ __auto_type __atomic_exchange_ptr = (
# 372 "./atomic/stdatomic-bitint-2.c"
     &v
# 372 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_val = (
# 372 "./atomic/stdatomic-bitint-2.c"
     5310030317361876753340683501073244375280341322936688061997080395958520351093955920782059690068348979597317732862920837580508090330213951646930178946470395395144632931547601wb
# 372 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_tmp; __atomic_exchange (__atomic_exchange_ptr, &__atomic_exchange_val, &__atomic_exchange_tmp, (5)); __atomic_exchange_tmp; })
      
# 373 "./atomic/stdatomic-bitint-2.c"
     != -285679222948993342888321238830899996788334328454283006589115162661816862491992700267590986826514460282130796141107636816730207482542791159837024986802592659048696136633096wb
      || 
# 374 "./atomic/stdatomic-bitint-2.c" 3 4
        __extension__ ({ __auto_type __atomic_load_ptr = (
# 374 "./atomic/stdatomic-bitint-2.c"
        &v
# 374 "./atomic/stdatomic-bitint-2.c" 3 4
        ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 374 "./atomic/stdatomic-bitint-2.c"
                         != 5310030317361876753340683501073244375280341322936688061997080395958520351093955920782059690068348979597317732862920837580508090330213951646930178946470395395144632931547601wb)
    abort ();
  if (
# 376 "./atomic/stdatomic-bitint-2.c" 3 4
     __extension__ ({ __auto_type __atomic_exchange_ptr = (
# 376 "./atomic/stdatomic-bitint-2.c"
     &v
# 376 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_val = (
# 376 "./atomic/stdatomic-bitint-2.c"
     -4427613371507571222951705350055906255006016685171218855857415580222001757304647519678682295710994407542058879878792272713121656498918585491540313864044915269100507689612649wb
# 376 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_tmp; __atomic_exchange (__atomic_exchange_ptr, &__atomic_exchange_val, &__atomic_exchange_tmp, (
# 376 "./atomic/stdatomic-bitint-2.c"
     memory_order_relaxed
# 376 "./atomic/stdatomic-bitint-2.c" 3 4
     )); __atomic_exchange_tmp; })

      
# 378 "./atomic/stdatomic-bitint-2.c"
     != 5310030317361876753340683501073244375280341322936688061997080395958520351093955920782059690068348979597317732862920837580508090330213951646930178946470395395144632931547601wb
      || (
# 379 "./atomic/stdatomic-bitint-2.c" 3 4
         __extension__ ({ __auto_type __atomic_load_ptr = (
# 379 "./atomic/stdatomic-bitint-2.c"
         &v
# 379 "./atomic/stdatomic-bitint-2.c" 3 4
         ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (
# 379 "./atomic/stdatomic-bitint-2.c"
         memory_order_acquire
# 379 "./atomic/stdatomic-bitint-2.c" 3 4
         )); __atomic_load_tmp; })
   
# 380 "./atomic/stdatomic-bitint-2.c"
  != -4427613371507571222951705350055906255006016685171218855857415580222001757304647519678682295710994407542058879878792272713121656498918585491540313864044915269100507689612649wb))
    abort ();

  count = 5310030317361876753340683501073244375280341322936688061997080395958520351093955920782059690068348979597317732862920837580508090330213951646930178946470395395144632931547601wb;
  if (
# 384 "./atomic/stdatomic-bitint-2.c" 3 4
     __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 384 "./atomic/stdatomic-bitint-2.c"
     &v
# 384 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 384 "./atomic/stdatomic-bitint-2.c"
     8894166061872682036448354332319628975793195843703235778626260413018342611721096976767565811189387129127069300488196032026080940074010266394902215699511191435840967345778707wb
# 384 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 384 "./atomic/stdatomic-bitint-2.c"
     &count
# 384 "./atomic/stdatomic-bitint-2.c" 3 4
     ), &__atomic_compare_exchange_tmp, 0, (5), (5)); })
                                                                                                                                                                                         
# 385 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                        )
    abort ();
  if (count != -4427613371507571222951705350055906255006016685171218855857415580222001757304647519678682295710994407542058879878792272713121656498918585491540313864044915269100507689612649wb
      || 
# 388 "./atomic/stdatomic-bitint-2.c" 3 4
        __extension__ ({ __auto_type __atomic_load_ptr = (
# 388 "./atomic/stdatomic-bitint-2.c"
        &v
# 388 "./atomic/stdatomic-bitint-2.c" 3 4
        ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 388 "./atomic/stdatomic-bitint-2.c"
                         != -4427613371507571222951705350055906255006016685171218855857415580222001757304647519678682295710994407542058879878792272713121656498918585491540313864044915269100507689612649wb)
    abort ();
  if (!
# 390 "./atomic/stdatomic-bitint-2.c" 3 4
      __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 390 "./atomic/stdatomic-bitint-2.c"
      &v
# 390 "./atomic/stdatomic-bitint-2.c" 3 4
      ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 390 "./atomic/stdatomic-bitint-2.c"
      8894166061872682036448354332319628975793195843703235778626260413018342611721096976767565811189387129127069300488196032026080940074010266394902215699511191435840967345778707wb
# 390 "./atomic/stdatomic-bitint-2.c" 3 4
      ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 390 "./atomic/stdatomic-bitint-2.c"
      &count
# 390 "./atomic/stdatomic-bitint-2.c" 3 4
      ), &__atomic_compare_exchange_tmp, 0, (5), (5)); })
                                                                                                                                                                                          
# 391 "./atomic/stdatomic-bitint-2.c"
                                                                                                                                                                                         )
    abort ();
  if (count != -4427613371507571222951705350055906255006016685171218855857415580222001757304647519678682295710994407542058879878792272713121656498918585491540313864044915269100507689612649wb
      || 
# 394 "./atomic/stdatomic-bitint-2.c" 3 4
        __extension__ ({ __auto_type __atomic_load_ptr = (
# 394 "./atomic/stdatomic-bitint-2.c"
        &v
# 394 "./atomic/stdatomic-bitint-2.c" 3 4
        ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 394 "./atomic/stdatomic-bitint-2.c"
                         != 8894166061872682036448354332319628975793195843703235778626260413018342611721096976767565811189387129127069300488196032026080940074010266394902215699511191435840967345778707wb)
    abort ();

  count = 5310030317361876753340683501073244375280341322936688061997080395958520351093955920782059690068348979597317732862920837580508090330213951646930178946470395395144632931547601wb;
  if (
# 398 "./atomic/stdatomic-bitint-2.c" 3 4
     __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 398 "./atomic/stdatomic-bitint-2.c"
     &v
# 398 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 398 "./atomic/stdatomic-bitint-2.c"
     -20263258027145541347005514871938569231358927704236809883668741851321168433670480594273940614283839738125120292424689828120954658857299483460682957837178666219043928324493674wb
# 398 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 398 "./atomic/stdatomic-bitint-2.c"
     &count
# 398 "./atomic/stdatomic-bitint-2.c" 3 4
     ), &__atomic_compare_exchange_tmp, 0, (
# 398 "./atomic/stdatomic-bitint-2.c"
     memory_order_seq_cst
# 398 "./atomic/stdatomic-bitint-2.c" 3 4
     ), (
# 398 "./atomic/stdatomic-bitint-2.c"
     memory_order_relaxed
# 398 "./atomic/stdatomic-bitint-2.c" 3 4
     )); })


                                 
# 401 "./atomic/stdatomic-bitint-2.c"
                                )
    abort ();
  if (count != 8894166061872682036448354332319628975793195843703235778626260413018342611721096976767565811189387129127069300488196032026080940074010266394902215699511191435840967345778707wb
      || 
# 404 "./atomic/stdatomic-bitint-2.c" 3 4
        __extension__ ({ __auto_type __atomic_load_ptr = (
# 404 "./atomic/stdatomic-bitint-2.c"
        &v
# 404 "./atomic/stdatomic-bitint-2.c" 3 4
        ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 404 "./atomic/stdatomic-bitint-2.c"
                         != 8894166061872682036448354332319628975793195843703235778626260413018342611721096976767565811189387129127069300488196032026080940074010266394902215699511191435840967345778707wb)
    abort ();
  if (!
# 406 "./atomic/stdatomic-bitint-2.c" 3 4
      __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 406 "./atomic/stdatomic-bitint-2.c"
      &v
# 406 "./atomic/stdatomic-bitint-2.c" 3 4
      ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 406 "./atomic/stdatomic-bitint-2.c"
      -20263258027145541347005514871938569231358927704236809883668741851321168433670480594273940614283839738125120292424689828120954658857299483460682957837178666219043928324493674wb
# 406 "./atomic/stdatomic-bitint-2.c" 3 4
      ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 406 "./atomic/stdatomic-bitint-2.c"
      &count
# 406 "./atomic/stdatomic-bitint-2.c" 3 4
      ), &__atomic_compare_exchange_tmp, 0, (
# 406 "./atomic/stdatomic-bitint-2.c"
      memory_order_seq_cst
# 406 "./atomic/stdatomic-bitint-2.c" 3 4
      ), (
# 406 "./atomic/stdatomic-bitint-2.c"
      memory_order_seq_cst
# 406 "./atomic/stdatomic-bitint-2.c" 3 4
      )); })


                           
# 409 "./atomic/stdatomic-bitint-2.c"
                          )
    abort ();
  if (count != 8894166061872682036448354332319628975793195843703235778626260413018342611721096976767565811189387129127069300488196032026080940074010266394902215699511191435840967345778707wb
      || 
# 412 "./atomic/stdatomic-bitint-2.c" 3 4
        __extension__ ({ __auto_type __atomic_load_ptr = (
# 412 "./atomic/stdatomic-bitint-2.c"
        &v
# 412 "./atomic/stdatomic-bitint-2.c" 3 4
        ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 412 "./atomic/stdatomic-bitint-2.c"
                         != -20263258027145541347005514871938569231358927704236809883668741851321168433670480594273940614283839738125120292424689828120954658857299483460682957837178666219043928324493674wb)
    abort ();

  count = 
# 415 "./atomic/stdatomic-bitint-2.c" 3 4
         __extension__ ({ __auto_type __atomic_load_ptr = (
# 415 "./atomic/stdatomic-bitint-2.c"
         &v
# 415 "./atomic/stdatomic-bitint-2.c" 3 4
         ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; })
# 415 "./atomic/stdatomic-bitint-2.c"
                         ;
  do
    res = count + 3438682542819842029328613486899299339199839206022263296398180581126218550036955367421469273900216774711772362599086182416923053671263751262393559525082445581168420546542404wb;
  while (!
# 418 "./atomic/stdatomic-bitint-2.c" 3 4
         __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 418 "./atomic/stdatomic-bitint-2.c"
         &v
# 418 "./atomic/stdatomic-bitint-2.c" 3 4
         ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 418 "./atomic/stdatomic-bitint-2.c"
         res
# 418 "./atomic/stdatomic-bitint-2.c" 3 4
         ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 418 "./atomic/stdatomic-bitint-2.c"
         &count
# 418 "./atomic/stdatomic-bitint-2.c" 3 4
         ), &__atomic_compare_exchange_tmp, 1, (5), (5)); })
# 418 "./atomic/stdatomic-bitint-2.c"
                                                       );
  if (
# 419 "./atomic/stdatomic-bitint-2.c" 3 4
     __extension__ ({ __auto_type __atomic_load_ptr = (
# 419 "./atomic/stdatomic-bitint-2.c"
     &v
# 419 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 419 "./atomic/stdatomic-bitint-2.c"
                      != -16824575484325699317676901385039269892159088498214546587270561270194949883633525226852471340383622963413347929825603645704031605186035732198289398312096220637875507777951270wb)
    abort ();

  count = 
# 422 "./atomic/stdatomic-bitint-2.c" 3 4
         __extension__ ({ __auto_type __atomic_load_ptr = (
# 422 "./atomic/stdatomic-bitint-2.c"
         &v
# 422 "./atomic/stdatomic-bitint-2.c" 3 4
         ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (
# 422 "./atomic/stdatomic-bitint-2.c"
         memory_order_acquire
# 422 "./atomic/stdatomic-bitint-2.c" 3 4
         )); __atomic_load_tmp; })
# 422 "./atomic/stdatomic-bitint-2.c"
                                                        ;
  do
    res = count + 55351299008567209999272942257960316150846002020561006740901388462123844029522178721330031429855007927083338249659817829635668878607777961512926342979905691183772323299904096wb;
  while (!
# 425 "./atomic/stdatomic-bitint-2.c" 3 4
         __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 425 "./atomic/stdatomic-bitint-2.c"
         &v
# 425 "./atomic/stdatomic-bitint-2.c" 3 4
         ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 425 "./atomic/stdatomic-bitint-2.c"
         res
# 425 "./atomic/stdatomic-bitint-2.c" 3 4
         ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 425 "./atomic/stdatomic-bitint-2.c"
         &count
# 425 "./atomic/stdatomic-bitint-2.c" 3 4
         ), &__atomic_compare_exchange_tmp, 1, (
# 425 "./atomic/stdatomic-bitint-2.c"
         memory_order_relaxed
# 425 "./atomic/stdatomic-bitint-2.c" 3 4
         ), (
# 425 "./atomic/stdatomic-bitint-2.c"
         memory_order_relaxed
# 425 "./atomic/stdatomic-bitint-2.c" 3 4
         )); })

                            
# 427 "./atomic/stdatomic-bitint-2.c"
                           );
  if (
# 428 "./atomic/stdatomic-bitint-2.c" 3 4
     __extension__ ({ __auto_type __atomic_load_ptr = (
# 428 "./atomic/stdatomic-bitint-2.c"
     &v
# 428 "./atomic/stdatomic-bitint-2.c" 3 4
     ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 428 "./atomic/stdatomic-bitint-2.c"
                      != 38526723524241510681596040872921046258686913522346460153630827191928894145888653494477560089471384963669990319834214183931637273421742229314636944667809470545896815521952826wb)
    abort ();
}


int
main ()
{

  test_fetch_add ();
  test_fetch_sub ();
  test_fetch_and ();
  test_fetch_xor ();
  test_fetch_or ();
  test_add ();
  test_sub ();
  test_and ();
  test_xor ();
  test_or ();
  test_exchange ();

  return 0;
}
