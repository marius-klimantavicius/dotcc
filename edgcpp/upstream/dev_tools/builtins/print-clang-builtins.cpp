/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/* Note that this doesn't compile with clang 3.5 or earlier. */

#include <optional>

#include "clang/Frontend/FrontendPluginRegistry.h"
#include "clang/AST/AST.h"
#include "clang/AST/ASTContext.h"
#include "clang/Frontend/CompilerInstance.h"
#include "clang/Sema/Sema.h"
#include "clang/Sema/Lookup.h"
#if __has_include("clang/Sema/SemaRISCV.h")
#include "clang/Sema/SemaRISCV.h"
#endif
#if __has_include("clang/Sema/RISCVIntrinsicManager.h")
#include "clang/Sema/RISCVIntrinsicManager.h"
#endif
#include "clang/Basic/Builtins.h"
#include "clang/Lex/Preprocessor.h"
#include "llvm/Config/llvm-config.h"
#include "llvm/Support/raw_ostream.h"

#if LLVM_VERSION_MAJOR >= 16 && \
    __has_include("clang/Support/RISCVVIntrinsicUtils.h") && \
    __has_include("clang/Basic/riscv_vector_builtin_sema.inc") && \
    __has_include("clang/Sema/RISCVIntrinsicManager.h")
#define PRINT_CLANG_RVV_C_API 1
#include "clang/Support/RISCVVIntrinsicUtils.h"
#include "llvm/ADT/StringSet.h"
#endif

using namespace clang;

namespace {

#if PRINT_CLANG_RVV_C_API
using namespace RISCV;
#if LLVM_VERSION_MAJOR >= 17
using IntrinsicKind = sema::RISCVIntrinsicManager::IntrinsicKind;
#endif

#if LLVM_VERSION_MAJOR >= 16
static llvm::SmallVector<PrototypeDescriptor>
RVVComputeBuiltinTypes(llvm::ArrayRef<PrototypeDescriptor> Prototype,
                       bool IsMasked, bool HasMaskedOffOperand, bool HasVL,
                       unsigned NF, PolicyScheme Scheme, Policy PolicyAttrs
#if LLVM_VERSION_MAJOR >= 17
                       ,
                       bool IsTuple
#endif
) {
#if LLVM_VERSION_MAJOR >= 17
  return RVVIntrinsic::computeBuiltinTypes(Prototype, IsMasked,
                                          HasMaskedOffOperand, HasVL, NF,
                                          Scheme, PolicyAttrs, IsTuple);
#else
  return RVVIntrinsic::computeBuiltinTypes(Prototype, IsMasked,
                                          HasMaskedOffOperand, HasVL, NF,
                                          Scheme, PolicyAttrs);
#endif
}

static void RVVUpdateNamesAndPolicy(bool IsMasked, bool HasPolicy,
                                    std::string &Name, std::string &BuiltinName,
                                    std::string &OverloadedName,
                                    Policy &PolicyAttrs,
                                    const RVVIntrinsicRecord &Record) {
#if LLVM_VERSION_MAJOR >= 22
  RVVIntrinsic::updateNamesAndPolicy(IsMasked, HasPolicy, Name, BuiltinName,
                                     OverloadedName, PolicyAttrs,
                                     Record.HasFRMRoundModeOp, Record.AltFmt);
#elif LLVM_VERSION_MAJOR >= 17
  RVVIntrinsic::updateNamesAndPolicy(IsMasked, HasPolicy, Name, BuiltinName,
                                     OverloadedName, PolicyAttrs,
                                     Record.HasFRMRoundModeOp);
#else
  RVVIntrinsic::updateNamesAndPolicy(IsMasked, HasPolicy, Name, BuiltinName,
                                     OverloadedName, PolicyAttrs);
#endif
}

static bool RVVIsTuple(const RVVIntrinsicRecord &Record) {
#if LLVM_VERSION_MAJOR >= 17
  return Record.IsTuple;
#else
  return false;
#endif
}

#endif // LLVM_VERSION_MAJOR >= 16

static const PrototypeDescriptor RVVSignatureTable[] = {
#define DECL_SIGNATURE_TABLE
#include "clang/Basic/riscv_vector_builtin_sema.inc"
#undef DECL_SIGNATURE_TABLE
};

static const RVVIntrinsicRecord RVVIntrinsicRecords[] = {
#define DECL_INTRINSIC_RECORDS
#include "clang/Basic/riscv_vector_builtin_sema.inc"
#undef DECL_INTRINSIC_RECORDS
};

#if __has_include("clang/Basic/riscv_sifive_vector_builtin_sema.inc")
static const PrototypeDescriptor RVSiFiveVectorSignatureTable[] = {
#define DECL_SIGNATURE_TABLE
#include "clang/Basic/riscv_sifive_vector_builtin_sema.inc"
#undef DECL_SIGNATURE_TABLE
};

static const RVVIntrinsicRecord RVSiFiveVectorIntrinsicRecords[] = {
#define DECL_INTRINSIC_RECORDS
#include "clang/Basic/riscv_sifive_vector_builtin_sema.inc"
#undef DECL_INTRINSIC_RECORDS
};
#endif

#if __has_include("clang/Basic/riscv_andes_vector_builtin_sema.inc")
static const PrototypeDescriptor RVAndesVectorSignatureTable[] = {
#define DECL_SIGNATURE_TABLE
#include "clang/Basic/riscv_andes_vector_builtin_sema.inc"
#undef DECL_SIGNATURE_TABLE
};

static const RVVIntrinsicRecord RVAndesVectorIntrinsicRecords[] = {
#define DECL_INTRINSIC_RECORDS
#include "clang/Basic/riscv_andes_vector_builtin_sema.inc"
#undef DECL_INTRINSIC_RECORDS
};
#endif

#if LLVM_VERSION_MAJOR >= 16
static ArrayRef<PrototypeDescriptor>
ProtoSeq2ArrayRef(uint16_t Index, uint8_t Length
#if LLVM_VERSION_MAJOR >= 17
                  ,
                  IntrinsicKind K
#endif
) {
#if LLVM_VERSION_MAJOR >= 17
  switch (K) {
  case IntrinsicKind::RVV:
    return ArrayRef(&RVVSignatureTable[Index], Length);
#if __has_include("clang/Basic/riscv_sifive_vector_builtin_sema.inc")
  case IntrinsicKind::SIFIVE_VECTOR:
    return ArrayRef(&RVSiFiveVectorSignatureTable[Index], Length);
#endif
#if __has_include("clang/Basic/riscv_andes_vector_builtin_sema.inc")
  case IntrinsicKind::ANDES_VECTOR:
    return ArrayRef(&RVAndesVectorSignatureTable[Index], Length);
#endif
  default:
    break;
  }
  llvm_unreachable("Unhandled IntrinsicKind");
#else
  return ArrayRef<PrototypeDescriptor>(&RVVSignatureTable[Index], Length);
#endif
}

#endif // LLVM_VERSION_MAJOR >= 16

// Collect C API names (mangled and overloaded) the same way SemaRISCV expands
// RVVIntrinsicRecords.  Clang only materializes these decls on lookup.
static void CollectRVVNames(ArrayRef<RVVIntrinsicRecord> Recs,
#if LLVM_VERSION_MAJOR >= 17
                            IntrinsicKind K,
#endif
                            RVVTypeCache &TypeCache,
                            llvm::StringSet<> &Names) {
  for (auto &Record : Recs) {
    ArrayRef<PrototypeDescriptor> BasicProtoSeq = ProtoSeq2ArrayRef(
        Record.PrototypeIndex, Record.PrototypeLength
#if LLVM_VERSION_MAJOR >= 17
        ,
        K
#endif
    );
    ArrayRef<PrototypeDescriptor> SuffixProto = ProtoSeq2ArrayRef(
        Record.SuffixIndex, Record.SuffixLength
#if LLVM_VERSION_MAJOR >= 17
        ,
        K
#endif
    );
    ArrayRef<PrototypeDescriptor> OverloadedSuffixProto = ProtoSeq2ArrayRef(
        Record.OverloadedSuffixIndex, Record.OverloadedSuffixSize
#if LLVM_VERSION_MAJOR >= 17
        ,
        K
#endif
    );

    PolicyScheme UnMaskedPolicyScheme =
        static_cast<PolicyScheme>(Record.UnMaskedPolicyScheme);
    PolicyScheme MaskedPolicyScheme =
        static_cast<PolicyScheme>(Record.MaskedPolicyScheme);
    const Policy DefaultPolicy;

    llvm::SmallVector<PrototypeDescriptor> ProtoSeq = RVVComputeBuiltinTypes(
        BasicProtoSeq, /*IsMasked=*/false,
        /*HasMaskedOffOperand=*/false, Record.HasVL, Record.NF,
        UnMaskedPolicyScheme, DefaultPolicy
#if LLVM_VERSION_MAJOR >= 17
        ,
        RVVIsTuple(Record)
#endif
    );

    bool UnMaskedHasPolicy = UnMaskedPolicyScheme != PolicyScheme::SchemeNone;
    bool MaskedHasPolicy = MaskedPolicyScheme != PolicyScheme::SchemeNone;
    SmallVector<Policy> SupportedUnMaskedPolicies =
        RVVIntrinsic::getSupportedUnMaskedPolicies();
    SmallVector<Policy> SupportedMaskedPolicies =
        RVVIntrinsic::getSupportedMaskedPolicies(Record.HasTailPolicy,
                                                 Record.HasMaskPolicy);

    auto AddNames = [&](StringRef SuffixStr, StringRef OverloadedSuffixStr,
                        bool IsMasked, bool HasPolicy, Policy PolicyAttrs) {
      std::string Name = Record.Name;
      if (!SuffixStr.empty())
        Name += "_" + SuffixStr.str();

      std::string OverloadedName;
      if (!Record.OverloadedName)
        OverloadedName = StringRef(Record.Name).split("_").first.str();
      else
        OverloadedName = Record.OverloadedName;
      if (!OverloadedSuffixStr.empty())
        OverloadedName += "_" + OverloadedSuffixStr.str();

      std::string BuiltinName = std::string(Record.Name);
      RVVUpdateNamesAndPolicy(IsMasked, HasPolicy, Name, BuiltinName,
                              OverloadedName, PolicyAttrs, Record);
      Names.insert(Name);
      if (!OverloadedName.empty())
        Names.insert(OverloadedName);
    };

    for (unsigned TypeRangeMaskShift = 0;
         TypeRangeMaskShift <= static_cast<unsigned>(BasicType::MaxOffset);
         ++TypeRangeMaskShift) {
      unsigned BaseTypeI = 1u << TypeRangeMaskShift;
      BasicType BaseType = static_cast<BasicType>(BaseTypeI);
      if ((BaseTypeI & Record.TypeRangeMask) != BaseTypeI)
        continue;

      for (int Log2LMUL = -3; Log2LMUL <= 3; Log2LMUL++) {
        if (!(Record.Log2LMULMask & (1 << (Log2LMUL + 3))))
          continue;

        std::optional<RVVTypes> Types =
            TypeCache.computeTypes(BaseType, Log2LMUL, Record.NF, ProtoSeq);
        if (!Types.has_value())
          continue;

        std::string SuffixStr = RVVIntrinsic::getSuffixStr(
            TypeCache, BaseType, Log2LMUL, SuffixProto);
        std::string OverloadedSuffixStr = RVVIntrinsic::getSuffixStr(
            TypeCache, BaseType, Log2LMUL, OverloadedSuffixProto);

        AddNames(SuffixStr, OverloadedSuffixStr, /*IsMasked=*/false,
                 UnMaskedHasPolicy, DefaultPolicy);

        if (Record.UnMaskedPolicyScheme != PolicyScheme::SchemeNone) {
          for (auto P : SupportedUnMaskedPolicies) {
            llvm::SmallVector<PrototypeDescriptor> PolicyPrototype =
                RVVComputeBuiltinTypes(
                    BasicProtoSeq, /*IsMasked=*/false,
                    /*HasMaskedOffOperand=*/false, Record.HasVL, Record.NF,
                    UnMaskedPolicyScheme, P
#if LLVM_VERSION_MAJOR >= 17
                    ,
                    RVVIsTuple(Record)
#endif
                );
            if (!TypeCache.computeTypes(BaseType, Log2LMUL, Record.NF,
                                        PolicyPrototype))
              continue;
            AddNames(SuffixStr, OverloadedSuffixStr, /*IsMasked=*/false,
                     UnMaskedHasPolicy, P);
          }
        }
        if (!Record.HasMasked)
          continue;

        llvm::SmallVector<PrototypeDescriptor> ProtoMaskSeq =
            RVVComputeBuiltinTypes(
                BasicProtoSeq, /*IsMasked=*/true, Record.HasMaskedOffOperand,
                Record.HasVL, Record.NF, MaskedPolicyScheme, DefaultPolicy
#if LLVM_VERSION_MAJOR >= 17
                ,
                RVVIsTuple(Record)
#endif
            );
        if (!TypeCache.computeTypes(BaseType, Log2LMUL, Record.NF, ProtoMaskSeq))
          continue;
        AddNames(SuffixStr, OverloadedSuffixStr, /*IsMasked=*/true,
                 MaskedHasPolicy, DefaultPolicy);

        if (Record.MaskedPolicyScheme == PolicyScheme::SchemeNone)
          continue;
        for (auto P : SupportedMaskedPolicies) {
          llvm::SmallVector<PrototypeDescriptor> PolicyPrototype =
              RVVComputeBuiltinTypes(
                  BasicProtoSeq, /*IsMasked=*/true, Record.HasMaskedOffOperand,
                  Record.HasVL, Record.NF, MaskedPolicyScheme, P
#if LLVM_VERSION_MAJOR >= 17
                  ,
                  RVVIsTuple(Record)
#endif
              );
          if (!TypeCache.computeTypes(BaseType, Log2LMUL, Record.NF,
                                      PolicyPrototype))
            continue;
          AddNames(SuffixStr, OverloadedSuffixStr, /*IsMasked=*/true,
                   MaskedHasPolicy, P);
        }
      }
    }
  }
}

static void PrintRVVFunctionDecl(FunctionDecl *FD, const PrintingPolicy &Policy,
                                 StringRef Table) {
  llvm::outs() << "/* " << Table << ":" << FD->getName() << " */ ";
  FD->getType().print(llvm::outs(), Policy, Twine(FD->getName()));
  llvm::outs() << ";\n";
}

static std::string RVVLookupName(StringRef Name) {
#if LLVM_VERSION_MAJOR >= 19
  if (Name.starts_with("__riscv_"))
#else
  if (Name.startswith("__riscv_"))
#endif
    return Name.str();
  std::string FullName = "__riscv_";
  FullName += Name.str();
  return FullName;
}

static void DumpRVVNameSet(Sema &S, Preprocessor &PP,
                           const PrintingPolicy &Policy,
                           const llvm::StringSet<> &Names, StringRef Table) {
  for (const auto &Entry : Names) {
    std::string FullName = RVVLookupName(Entry.getKey());
    IdentifierInfo &II = PP.getIdentifierTable().get(FullName);
    LookupResult LR(S, &II, SourceLocation(), Sema::LookupOrdinaryName);
    if (!S.LookupBuiltin(LR))
      continue;
    for (NamedDecl *ND : LR) {
      if (FunctionDecl *FD = dyn_cast<FunctionDecl>(ND))
        PrintRVVFunctionDecl(FD, Policy, Table);
    }
  }
}

struct RVVDeclareFlags {
  bool RVV;
  bool SiFive;
  bool Andes;
};

static bool IsRISCVMultiVectorTarget(const CompilerInstance &Instance) {
  StringRef Triple = Instance.getTargetOpts().Triple;
#if LLVM_VERSION_MAJOR >= 19
  return Triple.starts_with("riscv32") || Triple.starts_with("riscv64");
#else
  return Triple.startswith("riscv32") || Triple.startswith("riscv64");
#endif
}

static void EnableRVVTables(Sema &S, RVVDeclareFlags &Flags) {
#if __has_include("clang/Sema/SemaRISCV.h")
  S.RISCV().DeclareRVVBuiltins = true;
  Flags.RVV = true;
#if __has_include("clang/Basic/riscv_sifive_vector_builtin_sema.inc")
  S.RISCV().DeclareSiFiveVectorBuiltins = true;
  Flags.SiFive = true;
#endif
#if __has_include("clang/Basic/riscv_andes_vector_builtin_sema.inc")
  S.RISCV().DeclareAndesVectorBuiltins = true;
  Flags.Andes = true;
#endif
#else
  S.DeclareRISCVVBuiltins = true;
  Flags.RVV = true;
#if __has_include("clang/Basic/riscv_sifive_vector_builtin_sema.inc")
  S.DeclareRISCVSiFiveVectorBuiltins = true;
  Flags.SiFive = true;
#endif
#endif
}

static RVVDeclareFlags GetRVVDeclareFlags(Sema &S,
                                          const CompilerInstance &Instance) {
  RVVDeclareFlags Flags = {false, false, false};
  if (!IsRISCVMultiVectorTarget(Instance))
    return Flags;
  EnableRVVTables(S, Flags);
  return Flags;
}

static void DumpRISCVVectorCAPI(CompilerInstance &Instance,
                                const PrintingPolicy &Policy) {
  if (!Instance.hasSema())
    return;
  Sema &S = Instance.getSema();
  RVVDeclareFlags Declare = GetRVVDeclareFlags(S, Instance);
  if (!Declare.RVV && !Declare.SiFive && !Declare.Andes)
    return;

  Preprocessor &PP = Instance.getPreprocessor();
  RVVTypeCache TypeCache;
  auto DumpKind = [&](bool Declare, ArrayRef<RVVIntrinsicRecord> Recs,
#if LLVM_VERSION_MAJOR >= 17
                      IntrinsicKind K,
#endif
                      StringRef Table) {
    if (!Declare)
      return;
    llvm::StringSet<> Names;
    CollectRVVNames(Recs,
#if LLVM_VERSION_MAJOR >= 17
                    K,
#endif
                    TypeCache, Names);
    DumpRVVNameSet(S, PP, Policy, Names, Table);
  };

  DumpKind(Declare.RVV, RVVIntrinsicRecords,
#if LLVM_VERSION_MAJOR >= 17
           IntrinsicKind::RVV,
#endif
           "vector");
#if __has_include("clang/Basic/riscv_sifive_vector_builtin_sema.inc")
  DumpKind(Declare.SiFive, RVSiFiveVectorIntrinsicRecords,
#if LLVM_VERSION_MAJOR >= 17
           IntrinsicKind::SIFIVE_VECTOR,
#endif
           "sifive_vector");
#endif
#if __has_include("clang/Basic/riscv_andes_vector_builtin_sema.inc")
  DumpKind(Declare.Andes, RVAndesVectorIntrinsicRecords,
#if LLVM_VERSION_MAJOR >= 17
           IntrinsicKind::ANDES_VECTOR,
#endif
           "andes_vector");
#endif
}
#endif // PRINT_CLANG_RVV_C_API

class PrintBuiltinDeclarationsConsumer : public ASTConsumer {
  CompilerInstance &Instance;
  std::set<std::string> ParsedTemplates;

public:
  PrintBuiltinDeclarationsConsumer(CompilerInstance &Instance,
                                   std::set<std::string> ParsedTemplates)
      : Instance(Instance), ParsedTemplates(ParsedTemplates) {}

  // Invoked once per translation unit.
  void HandleTranslationUnit(ASTContext &Ctx) override {
    IdentifierTable &Table = Instance.getPreprocessor().getIdentifierTable();
    LangOptions LO;
    PrintingPolicy policy(LO);
    // For every entry in the identifier table:
    for (auto &TableEntry : Table) {
      auto IdentifierEntry = TableEntry.getValue();
      unsigned ID = IdentifierEntry->getBuiltinID();
      // If it's a builtin function:
      if (ID != 0) {
#if LLVM_VERSION_MAJOR >= 20
        // Clang 20.1.0+ crashes for builtins using the __mfp8 type for ARM32.
        // Most likely these shouldn't be defined for ARM32, so we ignore them
        // here.
        if (Instance.getTargetOpts().Triple.substr(0, 4) == "armv" &&
            (IdentifierEntry->getName().ends_with("_mf8") ||
             IdentifierEntry->getName().ends_with("_mf8_fpm") ||
             IdentifierEntry->getName().ends_with("_mf8_f16_fpm") ||
             IdentifierEntry->getName().ends_with("_mf8_f32_fpm"))) {
          continue;
        }
#endif
        ASTContext::GetBuiltinTypeError Error;
        // Get the builtin's type:
        QualType R = Ctx.GetBuiltinType(ID, Error);
        if (!Error) {
          // Write the function type (preceded by a comment with its name).
          llvm::outs() << "/* " << IdentifierEntry->getName() << " */ ";
          if (!R->isFunctionProtoType()) {
            // Treat un-prototyped types as functions with ellipses.
            const FunctionType *ft = dyn_cast<FunctionType>(R);
            llvm::outs() << ft->getReturnType().getAsString();
            llvm::outs() << " (" << IdentifierEntry->getName() << ")(...)";
          } else {
            // Emit the prototyped signature.
            R.print(llvm::outs(), policy, Twine(IdentifierEntry->getName()));
          }
          llvm::outs() << ";\n";
        }
      }
    }
#if PRINT_CLANG_RVV_C_API
    DumpRISCVVectorCAPI(Instance, policy);
#endif
  }

};

class PrintBuiltinDeclarations : public PluginASTAction {
  std::set<std::string> ParsedTemplates;
protected:
  std::unique_ptr<ASTConsumer> CreateASTConsumer(CompilerInstance &CI,
                                                 llvm::StringRef) override {
    return std::make_unique<PrintBuiltinDeclarationsConsumer>(CI,
                                                              ParsedTemplates);
  }

  bool ParseArgs(const CompilerInstance &CI,
                 const std::vector<std::string> &args) override {
    // Plugin takes no command-line arguments.
    return true;
  }
};

}

static FrontendPluginRegistry::Add<PrintBuiltinDeclarations>
X("print-builtins", "print builtin function declarations");
