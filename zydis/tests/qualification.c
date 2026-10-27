/* Test-only, translated C probes. All algorithms are the original Zydis core. */
#include <Zydis/Zydis.h>
#include <Zydis/Internal/String.h>
#include <Zycore/String.h>
#include <inttypes.h>
#include <stddef.h>
#include <stdio.h>
#include <string.h>

typedef struct ZqInfoCase_ { const char* name; int argc; const char* const* argv; } ZqInfoCase;
typedef struct ZqReencodeCase_
{
    ZydisMachineMode mode;
    ZydisStackWidth width;
    ZyanUSize size;
    ZyanU8 bytes[32];
} ZqReencodeCase;
#include "zq_fixtures.h"

typedef ZyanUSize (*ZqRead)(void*, ZyanU8*, ZyanUSize);
int ZqInfoMain(int argc, char** argv);
int ZqFuzzEncode(ZqRead reader, void* context);
int ZqFuzzReencode(ZqRead reader, void* context);
int ZqAbsoluteMain(void);

/* Unlike assert(), these checks also run with NDEBUG. */
#define ZQ_CHECK(expression) do { if (!(expression)) { \
    printf("CHECK FAILED %d: %s\n", __LINE__, #expression); return 1; } } while (0)
#define ZQ_OK(expression) do { ZyanStatus zq_s_ = (expression); \
    if (ZYAN_FAILED(zq_s_)) { printf("CHECK FAILED %d: %s status=%08" PRIX32 "\n", \
        __LINE__, #expression, zq_s_); return 1; } } while (0)
#define ZQ_STATUS(name, expression, expected) do { ZyanStatus zq_s_ = (expression); \
    printf("status:%s:%08" PRIX32 "\n", name, zq_s_); ZQ_CHECK(zq_s_ == (expected)); } while (0)
#define ZQ_COUNT(array) (sizeof(array) / sizeof((array)[0]))

/* _Alignof alone can become a compiler metadata literal. These global actual
 * wrapper types make alignment observable as emitted field storage offsets. */
#define ZQ_ALIGNMENT(type) typedef struct ZqAlignment_##type##_ { unsigned char prefix; type value; } ZqAlignment_##type
ZQ_ALIGNMENT(ZyanBool); ZQ_ALIGNMENT(ZyanUSize); ZQ_ALIGNMENT(ZyanStatus);
ZQ_ALIGNMENT(ZydisMachineMode); ZQ_ALIGNMENT(ZydisDecoder); ZQ_ALIGNMENT(ZydisDecoderContext);
ZQ_ALIGNMENT(ZydisDecodedInstruction); ZQ_ALIGNMENT(ZydisDecodedOperand);
ZQ_ALIGNMENT(ZydisEncoderOperand); ZQ_ALIGNMENT(ZydisEncoderRequest);
ZQ_ALIGNMENT(ZydisFormatter); ZQ_ALIGNMENT(ZydisFormatterContext);
ZQ_ALIGNMENT(ZydisFormatterBuffer); ZQ_ALIGNMENT(ZydisFormatterToken);
ZQ_ALIGNMENT(ZydisRegisterContext); ZQ_ALIGNMENT(ZydisDisassembledInstruction);
ZQ_ALIGNMENT(ZyanString); ZQ_ALIGNMENT(ZydisFormatterFunc);

static void ZqBytes(const char* label, const void* buffer, ZyanUSize size)
{
    const ZyanU8* bytes = buffer;
    printf("%s:", label);
    for (ZyanUSize i = 0; i < size; ++i) { printf("%02X", bytes[i]); }
    putchar('\n');
}

static int ZqAbi(void)
{
#define ZQ_LAYOUT(type) do { \
    ZQ_CHECK(offsetof(ZqAlignment_##type, value) == _Alignof(type)); \
    printf("layout:%s:%zu:%zu\n", #type, sizeof(type), offsetof(ZqAlignment_##type, value)); \
    } while (0)
#define ZQ_OFFSET(type, member) printf("offset:%s.%s:%zu\n", #type, #member, offsetof(type, member))
    ZQ_LAYOUT(ZyanBool); ZQ_LAYOUT(ZyanUSize); ZQ_LAYOUT(ZyanStatus);
    ZQ_LAYOUT(ZydisMachineMode); ZQ_LAYOUT(ZydisDecoder); ZQ_LAYOUT(ZydisDecoderContext);
    ZQ_LAYOUT(ZydisDecodedInstruction); ZQ_LAYOUT(ZydisDecodedOperand);
    ZQ_LAYOUT(ZydisEncoderOperand); ZQ_LAYOUT(ZydisEncoderRequest);
    ZQ_LAYOUT(ZydisFormatter); ZQ_LAYOUT(ZydisFormatterContext);
    ZQ_LAYOUT(ZydisFormatterBuffer); ZQ_LAYOUT(ZydisFormatterToken);
    ZQ_LAYOUT(ZydisRegisterContext); ZQ_LAYOUT(ZydisDisassembledInstruction);
    ZQ_LAYOUT(ZyanString); ZQ_LAYOUT(ZydisFormatterFunc);
    ZQ_OFFSET(ZydisDecoder, machine_mode); ZQ_OFFSET(ZydisDecoder, stack_width);
    ZQ_OFFSET(ZydisDecodedInstruction, mnemonic); ZQ_OFFSET(ZydisDecodedInstruction, attributes);
    ZQ_OFFSET(ZydisDecodedInstruction, avx); ZQ_OFFSET(ZydisDecodedInstruction, raw);
    ZQ_OFFSET(ZydisDecodedOperand, type); ZQ_OFFSET(ZydisDecodedOperand, reg);
    ZQ_OFFSET(ZydisDecodedOperand, mem); ZQ_OFFSET(ZydisDecodedOperand, ptr);
    ZQ_OFFSET(ZydisDecodedOperand, imm);
    ZQ_OFFSET(ZydisEncoderOperand, reg); ZQ_OFFSET(ZydisEncoderOperand, mem);
    ZQ_OFFSET(ZydisEncoderOperand, mem.displacement); ZQ_OFFSET(ZydisEncoderOperand, ptr);
    ZQ_OFFSET(ZydisEncoderOperand, imm);
    ZQ_OFFSET(ZydisEncoderRequest, prefixes); ZQ_OFFSET(ZydisEncoderRequest, operands);
    ZQ_OFFSET(ZydisEncoderRequest, evex); ZQ_OFFSET(ZydisEncoderRequest, mvex);
    ZQ_OFFSET(ZydisFormatter, func_print_mnemonic);
    ZQ_OFFSET(ZydisFormatterContext, user_data);
    ZQ_OFFSET(ZydisFormatterBuffer, string); ZQ_OFFSET(ZydisFormatterToken, next);
    ZQ_OFFSET(ZydisDisassembledInstruction, operands); ZQ_OFFSET(ZydisDisassembledInstruction, text);
    ZydisDecodedOperand operand;
    memset(&operand, 0, sizeof(operand));
    operand.type = ZYDIS_OPERAND_TYPE_IMMEDIATE;
    operand.imm.is_signed = ZYAN_TRUE;
    operand.imm.is_relative = ZYAN_TRUE;
    operand.imm.value.u = UINT64_C(0xFEDCBA9876543210);
    ZqBytes("storage:decoded-immediate", &operand, sizeof(operand));
    ZydisEncoderOperand encoder;
    memset(&encoder, 0, sizeof(encoder));
    encoder.type = ZYDIS_OPERAND_TYPE_MEMORY;
    encoder.reg.value = ZYDIS_REGISTER_RAX;
    encoder.reg.is4 = ZYAN_TRUE;
    encoder.mem.base = ZYDIS_REGISTER_RIP;
    encoder.mem.index = ZYDIS_REGISTER_RCX;
    encoder.mem.scale = 8;
    encoder.mem.displacement = -INT64_C(0x123456789);
    encoder.mem.size = 8;
    encoder.ptr.segment = 0xABCD;
    encoder.ptr.offset = 0x12345678;
    encoder.imm.u = UINT64_C(0xFFEEDDCCBBAA9988);
    ZqBytes("storage:encoder-operand", &encoder, sizeof(encoder));
    puts("CASE PASS abi");
    return 0;
}

static int ZqDecodeFormat(void)
{
    static const ZydisMachineMode modes[] = { ZYDIS_MACHINE_MODE_REAL_16,
        ZYDIS_MACHINE_MODE_LEGACY_16, ZYDIS_MACHINE_MODE_LONG_COMPAT_16,
        ZYDIS_MACHINE_MODE_LEGACY_32, ZYDIS_MACHINE_MODE_LONG_COMPAT_32, ZYDIS_MACHINE_MODE_LONG_64 };
    static const ZydisStackWidth widths[] = { ZYDIS_STACK_WIDTH_16, ZYDIS_STACK_WIDTH_16,
        ZYDIS_STACK_WIDTH_16, ZYDIS_STACK_WIDTH_32, ZYDIS_STACK_WIDTH_32, ZYDIS_STACK_WIDTH_64 };
    static const ZyanU8 bytes[] = { 0x8B, 0x44, 0x98, 0x10, 0xC3 };
    for (ZyanUSize m = 0; m < ZQ_COUNT(modes); ++m)
    {
        ZydisDecoder decoder;
        ZydisDecodedInstruction insn, split;
        ZydisDecoderContext context;
        ZydisDecodedOperand operands[ZYDIS_MAX_OPERAND_COUNT], separate[ZYDIS_MAX_OPERAND_COUNT];
        ZQ_OK(ZydisDecoderInit(&decoder, modes[m], widths[m]));
        ZQ_OK(ZydisDecoderDecodeFull(&decoder, bytes, sizeof(bytes), &insn, operands));
        ZQ_CHECK(insn.mnemonic == ZYDIS_MNEMONIC_MOV && insn.operand_count_visible == 2);
        ZQ_OK(ZydisDecoderDecodeInstruction(&decoder, &context, bytes, sizeof(bytes), &split));
        ZQ_OK(ZydisDecoderDecodeOperands(&decoder, &context, &split, separate, split.operand_count));
        ZQ_CHECK(split.mnemonic == insn.mnemonic && split.length == insn.length);
        for (ZyanU8 o = 0; o < insn.operand_count; ++o)
        {
            ZQ_CHECK(separate[o].type == operands[o].type && separate[o].size == operands[o].size);
        }
        printf("decode:%u:%u:%u:%u:%u:%u\n", (unsigned)modes[m], insn.length,
            insn.operand_width, insn.address_width, insn.stack_width, insn.operand_count);
        for (unsigned style = 0; style <= ZYDIS_FORMATTER_STYLE_INTEL_MASM; ++style)
        {
            ZydisFormatter formatter;
            char text[512], reconstructed[512];
            union { ZyanU64 aligned; ZyanU8 bytes[2048]; } tokens;
            ZydisFormatterTokenConst* token;
            ZyanUSize used = 0, count = 0;
            ZQ_OK(ZydisFormatterInit(&formatter, (ZydisFormatterStyle)style));
            ZQ_OK(ZydisFormatterSetProperty(&formatter, ZYDIS_FORMATTER_PROP_FORCE_SIZE, ZYAN_TRUE));
            ZQ_OK(ZydisFormatterFormatInstruction(&formatter, &insn, operands,
                insn.operand_count_visible, text, sizeof(text), 0x4000, NULL));
            printf("format:%u:%u:%s\n", (unsigned)modes[m], style, text);
            ZQ_OK(ZydisFormatterTokenizeInstruction(&formatter, &insn, operands,
                insn.operand_count_visible, tokens.bytes, sizeof(tokens), 0x4000, &token, NULL));
            for (;;)
            {
                ZydisTokenType type;
                const char* value;
                ZQ_CHECK((const ZyanU8*)token >= tokens.bytes &&
                    (const ZyanU8*)token < tokens.bytes + sizeof(tokens));
                ZQ_OK(ZydisFormatterTokenGetValue(token, &type, &value));
                ZyanUSize n = strlen(value);
                ZQ_CHECK(used + n < sizeof(reconstructed) && ++count < 100);
                memcpy(reconstructed + used, value, n); used += n;
                printf("token:%u:%s\n", (unsigned)type, value);
                ZyanStatus next = ZydisFormatterTokenNext(&token);
                if (ZYAN_FAILED(next))
                {
                    printf("token-end:%08" PRIX32 "\n", next);
                    ZQ_CHECK(next == ZYAN_STATUS_OUT_OF_RANGE);
                    break;
                }
            }
            reconstructed[used] = 0;
            ZQ_CHECK(strcmp(reconstructed, text) == 0);
            for (ZyanU8 o = 0; o < insn.operand_count_visible; ++o)
            {
                ZQ_OK(ZydisFormatterFormatOperand(&formatter, &insn, &operands[o],
                    text, sizeof(text), 0x4000, NULL));
                printf("operand:%u:%s\n", o, text);
                ZQ_OK(ZydisFormatterTokenizeOperand(&formatter, &insn, &operands[o],
                    tokens.bytes, sizeof(tokens), 0x4000, &token, NULL));
            }
        }
        ZydisDisassembledInstruction simple;
        ZQ_OK(ZydisDisassembleIntel(modes[m], 0x4000, bytes, sizeof(bytes), &simple));
        printf("simple-intel:%s\n", simple.text);
        ZQ_OK(ZydisDisassembleATT(modes[m], 0x4000, bytes, sizeof(bytes), &simple));
        printf("simple-att:%s\n", simple.text);
    }
    puts("CASE PASS decode-format-tokenize-modes");
    return 0;
}

typedef struct ZqHookContext_
{
    ZyanU64 magic;
    ZyanU32 calls;
    ZyanStatus result;
    ZydisFormatterFunc original;
    const char* text;
} ZqHookContext;

static ZyanStatus ZqMnemonicHook(const ZydisFormatter* formatter,
    ZydisFormatterBuffer* buffer, ZydisFormatterContext* context)
{
    ZqHookContext* state = context ? context->user_data : NULL;
    if (!state || state->magic != UINT64_C(0x71A9BD023456CDEF) || !state->original)
        return ZYAN_STATUS_INVALID_ARGUMENT;
    ++state->calls;
    if (ZYAN_FAILED(state->result)) { return state->result; }
    ZYAN_CHECK(state->original(formatter, buffer, context));
    ZyanString* string;
    ZYAN_CHECK(ZydisFormatterBufferGetString(buffer, &string));
    ZyanStringView suffix;
    memset(&suffix, 0, sizeof(suffix));
    suffix.string.vector.data = (void*)state->text;
    suffix.string.vector.size = strlen(state->text) + 1;
    return ZydisStringAppend(string, &suffix);
}

static int ZqHooks(void)
{
    static const ZyanU8 bytes[] = { 0x48, 0x89, 0xD8 };
    ZydisDecoder decoder;
    ZydisDecodedInstruction insn;
    ZydisDecodedOperand operands[ZYDIS_MAX_OPERAND_COUNT];
    ZydisFormatter first, second;
    ZqHookContext a = { UINT64_C(0x71A9BD023456CDEF), 0, ZYAN_STATUS_SUCCESS, NULL, ".first" };
    ZqHookContext b = { UINT64_C(0x71A9BD023456CDEF), 0, ZYAN_STATUS_SUCCESS, NULL, ".second" };
    char retained[256], current[256];
    const void* hook = (const void*)&ZqMnemonicHook;
    ZQ_OK(ZydisDecoderInit(&decoder, ZYDIS_MACHINE_MODE_LONG_64, ZYDIS_STACK_WIDTH_64));
    ZQ_OK(ZydisDecoderDecodeFull(&decoder, bytes, sizeof(bytes), &insn, operands));
    ZQ_OK(ZydisFormatterInit(&first, ZYDIS_FORMATTER_STYLE_INTEL));
    ZQ_OK(ZydisFormatterInit(&second, ZYDIS_FORMATTER_STYLE_ATT));
    ZQ_OK(ZydisFormatterSetHook(&first, ZYDIS_FORMATTER_FUNC_PRINT_MNEMONIC, &hook));
    a.original = (ZydisFormatterFunc)hook;
    hook = (const void*)&ZqMnemonicHook;
    ZQ_OK(ZydisFormatterSetHook(&second, ZYDIS_FORMATTER_FUNC_PRINT_MNEMONIC, &hook));
    b.original = (ZydisFormatterFunc)hook;
    hook = NULL;
    ZQ_OK(ZydisFormatterSetHook(&first, ZYDIS_FORMATTER_FUNC_PRINT_MNEMONIC, &hook));
    ZQ_CHECK(hook == (const void*)&ZqMnemonicHook);
    for (unsigned i = 0; i < 12; ++i)
    {
        ZQ_OK(ZydisFormatterFormatInstruction(&first, &insn, operands, insn.operand_count_visible,
            retained, sizeof(retained), 0x4000, &a));
        ZQ_OK(ZydisFormatterFormatInstruction(&second, &insn, operands, insn.operand_count_visible,
            current, sizeof(current), 0x4000, &b));
        ZQ_CHECK(strstr(retained, ".first") && !strstr(retained, ".second"));
        ZQ_CHECK(strstr(current, ".second") && !strstr(current, ".first"));
    }
    ZQ_CHECK(a.calls == 12 && b.calls == 12);
    printf("hooks:%u:%u:%s:%s\n", a.calls, b.calls, retained, current);
    a.result = ZYAN_STATUS_INVALID_OPERATION;
    ZQ_STATUS("hook-failure", ZydisFormatterFormatInstruction(&first, &insn, operands,
        insn.operand_count_visible, current, sizeof(current), 0x4000, &a), ZYAN_STATUS_INVALID_OPERATION);
    hook = (const void*)a.original;
    ZQ_OK(ZydisFormatterSetHook(&first, ZYDIS_FORMATTER_FUNC_PRINT_MNEMONIC, &hook));
    ZQ_CHECK(hook == (const void*)&ZqMnemonicHook);
    a.magic = 0; /* Context ends after the synchronous hook is removed. */
    ZQ_OK(ZydisFormatterFormatInstruction(&first, &insn, operands, insn.operand_count_visible,
        current, sizeof(current), 0x4000, NULL));
    ZQ_CHECK(!strstr(current, ".first") && a.calls == 13);
    printf("unhooked:%s;retained:%s\n", current, retained);
    puts("CASE PASS formatter-hook-context-lifetimes");
    return 0;
}

static int ZqErrors(void)
{
    ZydisDecoder decoder;
    ZydisDecodedInstruction insn;
    ZydisDecodedOperand operands[ZYDIS_MAX_OPERAND_COUNT];
    ZydisFormatter formatter;
    ZyanU8 prefix[] = { 0x0F }, illegal_lock[] = { 0xF0, 0x90 }, long_insn[16];
    char buffer[1];
    ZQ_STATUS("decoder-null", ZydisDecoderInit(NULL, ZYDIS_MACHINE_MODE_LONG_64,
        ZYDIS_STACK_WIDTH_64), ZYAN_STATUS_INVALID_ARGUMENT);
    ZQ_STATUS("decoder-bad-mode", ZydisDecoderInit(&decoder, (ZydisMachineMode)99,
        ZYDIS_STACK_WIDTH_64), ZYAN_STATUS_INVALID_ARGUMENT);
    ZQ_OK(ZydisDecoderInit(&decoder, ZYDIS_MACHINE_MODE_LONG_64, ZYDIS_STACK_WIDTH_64));
    ZQ_STATUS("empty", ZydisDecoderDecodeFull(&decoder, prefix, 0, &insn, operands), ZYDIS_STATUS_NO_MORE_DATA);
    ZQ_STATUS("truncated", ZydisDecoderDecodeFull(&decoder, prefix, sizeof(prefix), &insn, operands), ZYDIS_STATUS_NO_MORE_DATA);
    memset(long_insn, 0x66, sizeof(long_insn));
    ZQ_STATUS("overlong", ZydisDecoderDecodeFull(&decoder, long_insn, sizeof(long_insn), &insn, operands), ZYDIS_STATUS_INSTRUCTION_TOO_LONG);
    ZQ_STATUS("illegal-lock", ZydisDecoderDecodeFull(&decoder, illegal_lock, sizeof(illegal_lock), &insn, operands), ZYDIS_STATUS_ILLEGAL_LOCK);
    ZQ_STATUS("decoder-bad-option", ZydisDecoderEnableMode(&decoder, (ZydisDecoderMode)99, ZYAN_TRUE), ZYAN_STATUS_INVALID_ARGUMENT);
    ZQ_STATUS("formatter-bad-style", ZydisFormatterInit(&formatter, (ZydisFormatterStyle)99), ZYAN_STATUS_INVALID_ARGUMENT);
    ZQ_OK(ZydisFormatterInit(&formatter, ZYDIS_FORMATTER_STYLE_INTEL));
    ZQ_STATUS("formatter-bad-property", ZydisFormatterSetProperty(&formatter, (ZydisFormatterProperty)999, 0), ZYAN_STATUS_INVALID_ARGUMENT);
    const ZyanU8 mov[] = { 0x48, 0x89, 0xD8 };
    ZQ_OK(ZydisDecoderDecodeFull(&decoder, mov, sizeof(mov), &insn, operands));
    ZQ_STATUS("formatter-small-buffer", ZydisFormatterFormatInstruction(&formatter, &insn, operands,
        insn.operand_count_visible, buffer, sizeof(buffer), 0x4000, NULL), ZYAN_STATUS_INSUFFICIENT_BUFFER_SIZE);
    ZydisEncoderRequest request;
    memset(&request, 0, sizeof(request));
    request.machine_mode = ZYDIS_MACHINE_MODE_LONG_64;
    ZyanU8 output[32]; ZyanUSize length = sizeof(output);
    ZQ_STATUS("encoder-invalid-mnemonic", ZydisEncoderEncodeInstruction(&request, output, &length), ZYAN_STATUS_INVALID_ARGUMENT);
    request.mnemonic = ZYDIS_MNEMONIC_NOP; length = 0;
    ZQ_STATUS("encoder-small-buffer", ZydisEncoderEncodeInstruction(&request, output, &length), ZYAN_STATUS_INSUFFICIENT_BUFFER_SIZE);
    puts("CASE PASS ordinary-errors");
    return 0;
}

static int ZqEncodeUtilities(void)
{
    static const ZydisMachineMode modes[] = { ZYDIS_MACHINE_MODE_LEGACY_16,
        ZYDIS_MACHINE_MODE_LEGACY_32, ZYDIS_MACHINE_MODE_LONG_64 };
    static const ZydisStackWidth widths[] = { ZYDIS_STACK_WIDTH_16, ZYDIS_STACK_WIDTH_32, ZYDIS_STACK_WIDTH_64 };
    static const ZydisRegister regs[] = { ZYDIS_REGISTER_AX, ZYDIS_REGISTER_EAX, ZYDIS_REGISTER_RAX };
    for (unsigned m = 0; m < ZQ_COUNT(modes); ++m)
    {
        ZydisEncoderRequest request, converted;
        memset(&request, 0, sizeof(request));
        request.machine_mode = modes[m]; request.mnemonic = ZYDIS_MNEMONIC_MOV; request.operand_count = 2;
        request.operands[0].type = ZYDIS_OPERAND_TYPE_REGISTER; request.operands[0].reg.value = regs[m];
        request.operands[1].type = ZYDIS_OPERAND_TYPE_IMMEDIATE; request.operands[1].imm.u = 0x1234;
        ZyanU8 bytes[15], reencoded[15];
        ZyanUSize size = sizeof(bytes), size2 = sizeof(reencoded);
        ZydisDecoder decoder;
        ZydisDecodedInstruction insn;
        ZydisDecodedOperand operands[ZYDIS_MAX_OPERAND_COUNT];
        ZQ_OK(ZydisEncoderEncodeInstruction(&request, bytes, &size));
        ZqBytes("encode-mov", bytes, size);
        ZQ_OK(ZydisDecoderInit(&decoder, modes[m], widths[m]));
        ZQ_OK(ZydisDecoderDecodeFull(&decoder, bytes, size, &insn, operands));
        ZQ_CHECK(insn.mnemonic == request.mnemonic && operands[0].reg.value == regs[m] && operands[1].imm.value.u == 0x1234);
        ZQ_OK(ZydisEncoderDecodedInstructionToEncoderRequest(&insn, operands, insn.operand_count_visible, &converted));
        ZQ_OK(ZydisEncoderEncodeInstruction(&converted, reencoded, &size2));
        ZQ_CHECK(size == size2 && memcmp(bytes, reencoded, size) == 0);
        memset(&request, 0, sizeof(request));
        request.machine_mode = modes[m]; request.mnemonic = ZYDIS_MNEMONIC_JMP; request.operand_count = 1;
        request.operands[0].type = ZYDIS_OPERAND_TYPE_IMMEDIATE; request.operands[0].imm.u = 0x4070;
        size = sizeof(bytes);
        ZQ_OK(ZydisEncoderEncodeInstructionAbsolute(&request, bytes, &size, 0x4000));
        ZqBytes("absolute-jmp", bytes, size);
        ZQ_OK(ZydisDecoderDecodeFull(&decoder, bytes, size, &insn, operands));
        ZyanU64 address = 0;
        ZQ_OK(ZydisCalcAbsoluteAddress(&insn, &operands[0], 0x4000, &address));
        ZQ_CHECK(address == 0x4070);
        printf("absolute:%" PRIX64 "\n", address);
    }
    puts("CASE PASS encode-convert-roundtrip-absolute");
    for (ZyanUSize n = 0; n <= 64; ++n)
    {
        ZyanU8 bytes[66]; memset(bytes, 0xA5, sizeof(bytes));
        ZQ_OK(ZydisEncoderNopFill(bytes + 1, n));
        ZQ_CHECK(bytes[0] == 0xA5 && bytes[n + 1] == 0xA5);
        ZqBytes("nop", bytes + 1, n);
        ZydisDecoder decoder; ZQ_OK(ZydisDecoderInit(&decoder, ZYDIS_MACHINE_MODE_LONG_64, ZYDIS_STACK_WIDTH_64));
        for (ZyanUSize used = 0; used < n;)
        {
            ZydisDecodedInstruction insn; ZydisDecodedOperand operands[ZYDIS_MAX_OPERAND_COUNT];
            ZQ_OK(ZydisDecoderDecodeFull(&decoder, bytes + 1 + used, n - used, &insn, operands));
            ZQ_CHECK(insn.mnemonic == ZYDIS_MNEMONIC_NOP && insn.length && insn.length <= n - used);
            used += insn.length;
        }
    }
    puts("CASE PASS nop-fill");
    const ZyanU8 load[] = { 0x48, 0x8B, 0x44, 0x8B, 0x10 };
    ZydisDecoder decoder; ZydisDecodedInstruction insn;
    ZydisDecodedOperand operands[ZYDIS_MAX_OPERAND_COUNT]; ZydisRegisterContext registers;
    memset(&registers, 0, sizeof(registers)); registers.values[ZYDIS_REGISTER_RBX] = 0x1000;
    registers.values[ZYDIS_REGISTER_RCX] = 3;
    ZQ_OK(ZydisDecoderInit(&decoder, ZYDIS_MACHINE_MODE_LONG_64, ZYDIS_STACK_WIDTH_64));
    ZQ_OK(ZydisDecoderDecodeFull(&decoder, load, sizeof(load), &insn, operands));
    ZyanU64 address = 0;
    ZQ_OK(ZydisCalcAbsoluteAddressEx(&insn, &operands[1], 0x4000, &registers, &address));
    ZQ_CHECK(address == 0x101C); printf("register-context:%" PRIX64 "\n", address);
    ZydisInstructionSegments segments;
    ZQ_OK(ZydisGetInstructionSegments(&insn, &segments));
    for (ZyanU8 i = 0; i < segments.count; ++i)
    {
        ZQ_CHECK(segments.segments[i].offset + segments.segments[i].size <= insn.length);
        printf("segment:%u:%u:%u\n", (unsigned)segments.segments[i].type,
            segments.segments[i].offset, segments.segments[i].size);
    }
    ZQ_CHECK(ZydisRegisterGetWidth(ZYDIS_MACHINE_MODE_LONG_64, ZYDIS_REGISTER_RAX) == 64);
    ZQ_CHECK(ZydisRegisterGetLargestEnclosing(ZYDIS_MACHINE_MODE_LONG_64, ZYDIS_REGISTER_AL) == ZYDIS_REGISTER_RAX);
    ZQ_CHECK(strcmp(ZydisRegisterGetString(ZYDIS_REGISTER_RAX), "rax") == 0);
    ZQ_CHECK(strcmp(ZydisMnemonicGetString(ZYDIS_MNEMONIC_MOV), "mov") == 0);
    printf("meta:%s:%s:%s\n", ZydisCategoryGetString(insn.meta.category),
        ZydisISASetGetString(insn.meta.isa_set), ZydisISAExtGetString(insn.meta.isa_ext));
    puts("CASE PASS utilities-register-context-segments");
    return 0;
}

typedef struct ZqStream_ { const ZyanU8* bytes; ZyanUSize size; ZyanUSize offset; } ZqStream;
static ZyanUSize ZqReadMemory(void* context, ZyanU8* output, ZyanUSize capacity)
{
    ZqStream* stream = context;
    ZyanUSize count = stream->size - stream->offset;
    if (count > capacity) { count = capacity; }
    memcpy(output, stream->bytes + stream->offset, count); stream->offset += count;
    return count;
}

static int ZqInfo(void)
{
    for (ZyanUSize i = 0; i < ZQ_COUNT(zq_info_cases); ++i)
    {
        const ZqInfoCase* test = &zq_info_cases[i];
        char storage[ZQ_INFO_MAX_ARGC][ZQ_INFO_MAX_ARGUMENT];
        char* argv[ZQ_INFO_MAX_ARGC + 1];
        for (int a = 0; a < test->argc; ++a)
        {
            ZQ_CHECK(strlen(test->argv[a]) < sizeof(storage[a]));
            strcpy(storage[a], test->argv[a]); argv[a] = storage[a];
        }
        argv[test->argc] = NULL;
        printf("@@ZQ CASE %s BEGIN\n", test->name);
        fflush(stdout);
        /* Original ZydisInfo strips whitespace in-place in its argv strings. */
        int result = ZqInfoMain(test->argc, argv);
        printf("@@ZQ CASE %s END %d\n", test->name, result);
        /* Upstream regression.py compares stdout only. Three original empty
         * snapshots reject their bytes. The Python runner independently runs
         * the unrenamed original CLI and validates exact status and stderr. */
    }
    puts("CASE PASS upstream-info");
    return 0;
}

static int ZqEncoderFixtures(void)
{
    for (ZyanUSize i = 0; i < ZQ_COUNT(zq_encoder_cases); ++i)
    {
        printf("@@ZQ CASE encoder/%03zu BEGIN\n", i);
        fflush(stdout);
        ZydisEncoderRequest raw = zq_encoder_cases[i];
        ZyanU8 encoded[15]; ZyanUSize length = sizeof(encoded);
        ZyanStatus status = ZydisEncoderEncodeInstruction(&raw, encoded, &length);
        printf("encode-status:%08" PRIX32 "\n", status);
        if (ZYAN_SUCCESS(status)) { ZqBytes("encoded", encoded, length); }
        /* The original fuzz runner stores operand types offset by REGISTER.
         * Transform actual fields, rather than guessing any struct layout. */
        for (ZyanU8 o = 0; o < raw.operand_count; ++o)
            raw.operands[o].type = (ZydisOperandType)(raw.operands[o].type - ZYDIS_OPERAND_TYPE_REGISTER);
        ZqStream stream = { (const ZyanU8*)&raw, sizeof(raw), 0 };
        int result = ZqFuzzEncode(&ZqReadMemory, &stream);
        ZQ_CHECK(stream.offset == sizeof(raw));
        printf("@@ZQ CASE encoder/%03zu END %d\n", i, result);
        ZQ_CHECK(result == 0);
    }
    for (ZyanUSize i = 0; i < ZQ_COUNT(zq_reencoder_cases); ++i)
    {
        printf("@@ZQ CASE reencoder/%03zu BEGIN\n", i);
        fflush(stdout);
        const ZqReencodeCase* test = &zq_reencoder_cases[i];
        /* These are the actual enum objects consumed by upstream's reader. */
        ZyanU8 payload[sizeof(test->mode) + sizeof(test->width) + sizeof(test->bytes)];
        memcpy(payload, &test->mode, sizeof(test->mode));
        memcpy(payload + sizeof(test->mode), &test->width, sizeof(test->width));
        memcpy(payload + sizeof(test->mode) + sizeof(test->width), test->bytes, test->size);
        ZqStream stream = { payload, sizeof(test->mode) + sizeof(test->width) + test->size, 0 };
        int result = ZqFuzzReencode(&ZqReadMemory, &stream);
        ZQ_CHECK(stream.offset == stream.size);
        printf("@@ZQ CASE reencoder/%03zu END %d\n", i, result);
        ZQ_CHECK(result == 0);
    }
    puts("CASE PASS upstream-encoder-reencoder");
    return 0;
}

int ZqRun(unsigned group)
{
    switch (group)
    {
    case 0: return ZqAbi();
    case 1:
        ZQ_CHECK(ZydisGetVersion() == ZYDIS_VERSION);
        printf("version:%016" PRIX64 "\n", ZydisGetVersion());
        for (unsigned feature = 0; feature <= ZYDIS_FEATURE_MAX_VALUE; ++feature)
            printf("feature:%u:%08" PRIX32 "\n", feature, ZydisIsFeatureEnabled((ZydisFeature)feature));
        puts("CASE PASS version-features");
        if (ZqDecodeFormat() || ZqHooks() || ZqErrors() || ZqEncodeUtilities()) { return 1; }
        return 0;
    case 2: return ZqInfo();
    case 3: return ZqEncoderFixtures();
    case 4:
    {
        puts("@@ZQ CASE absolute/original BEGIN");
        fflush(stdout);
        int result = ZqAbsoluteMain();
        printf("@@ZQ CASE absolute/original END %d\n", result);
        ZQ_CHECK(result == 0);
        puts("CASE PASS upstream-absolute"); return 0;
    }
    case 5: ZQ_CHECK(0 && "deliberate failure control");
    default: fputs("Invalid qualification group\n", stderr); return 2;
    }
}
