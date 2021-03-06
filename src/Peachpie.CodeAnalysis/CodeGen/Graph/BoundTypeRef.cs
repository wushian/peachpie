﻿using Devsense.PHP.Syntax.Ast;
using Microsoft.CodeAnalysis;
using Pchp.CodeAnalysis.CodeGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Pchp.CodeAnalysis.Symbols;

namespace Pchp.CodeAnalysis.Semantics
{
    partial class BoundTypeRef
    {
        /// <summary>
        /// Emits name of bound type.
        /// </summary>
        /// <param name="cg"></param>
        internal void EmitClassName(CodeGenerator cg)
        {
            if (TypeExpression != null)
            {
                cg.EmitConvert(TypeExpression, cg.CoreTypes.String);
            }
            else
            {
                if (_typeRef is PrimitiveTypeRef)
                {
                    throw new InvalidOperationException();
                }
                else if (_typeRef is TranslatedTypeRef || _typeRef is ClassTypeRef)
                {
                    var classname = ((INamedTypeRef)_typeRef).ClassName;
                    cg.Builder.EmitStringConstant(classname.ToString());
                }
                else
                {
                    throw Roslyn.Utilities.ExceptionUtilities.UnexpectedValue(_typeRef);
                }
            }
        }

        /// <summary>
        /// Emits load of <c>PhpTypeInfo</c>.
        /// </summary>
        /// <param name="cg">Code generator instance.</param>
        /// <param name="throwOnError">Emits PHP error in case type is not declared.</param>
        /// <remarks>Emits <c>NULL</c> in case type is not declared.</remarks>
        internal TypeSymbol EmitLoadTypeInfo(CodeGenerator cg, bool throwOnError = false)
        {
            Debug.Assert(cg != null);

            TypeSymbol t;

            if (this.ResolvedType != null)
            {
                t = (TypeSymbol)EmitLoadPhpTypeInfo(cg, this.ResolvedType);
            }
            else
            {
                // CALL <ctx>.GetDeclaredType(<typename>, autoload:true)
                cg.EmitLoadContext();
                this.EmitClassName(cg);
                cg.Builder.EmitBoolConstant(true);
                t = cg.EmitCall(ILOpCode.Call, throwOnError
                    ? cg.CoreMethods.Context.GetDeclaredTypeOrThrow_string_bool
                    : cg.CoreMethods.Context.GetDeclaredType_string_bool);
            }

            return t;
        }

        internal static ITypeSymbol EmitLoadPhpTypeInfo(CodeGenerator cg, ITypeSymbol t)
        {
            Contract.ThrowIfNull(t);

            // CALL GetPhpTypeInfo<T>()
            return cg.EmitCall(ILOpCode.Call, cg.CoreMethods.Dynamic.GetPhpTypeInfo_T.Symbol.Construct(t));
        }

        public void Accept(PhpOperationVisitor visitor) => visitor.VisitTypeRef(this);
    }
}
