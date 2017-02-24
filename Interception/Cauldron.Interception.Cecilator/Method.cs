﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Cauldron.Interception.Cecilator
{
    public class Method : CecilatorBase, IEquatable<Method>
    {
        [EditorBrowsable(EditorBrowsableState.Never), DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal readonly MethodDefinition methodDefinition;

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal readonly MethodReference methodReference;

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal readonly BuilderType type;

        internal Method(BuilderType type, MethodReference methodReference, MethodDefinition methodDefinition) : base(type)
        {
            this.type = type;
            this.methodDefinition = methodDefinition;
            this.methodReference = methodReference;
        }

        internal Method(BuilderType type, MethodDefinition methodDefinition) : base(type)
        {
            this.type = type;
            this.methodDefinition = methodDefinition;
            this.methodReference = methodDefinition.CreateMethodReference();
        }

        public BuilderType DeclaringType { get { return this.type; } }

        public bool IsAbstract { get { return this.methodDefinition.IsAbstract; } }

        public bool IsCCtor { get { return this.methodDefinition.Name == ".cctor"; } }

        public bool IsCtor { get { return this.methodDefinition.Name == ".ctor"; } }

        public bool IsPublic { get { return this.methodDefinition.Attributes.HasFlag(MethodAttributes.Public); } }

        public bool IsStatic { get { return this.methodDefinition.IsStatic; } }

        public bool IsVoid { get { return this.methodDefinition.ReturnType.FullName == "System.Void"; } }

        public string Name { get { return this.methodDefinition.Name; } }

        public BuilderType[] Parameters { get { return this.methodReference.Parameters.Select(x => new BuilderType(this.DeclaringType.Builder, x.ParameterType)).ToArray(); } }

        public BuilderType ReturnType { get { return new BuilderType(this.type, this.methodReference.ReturnType); } }

        public Field CreateField(Type fieldType, string name) =>
            this.CreateField(this.moduleDefinition.Import(this.GetTypeDefinition(fieldType).ResolveType(this.DeclaringType.typeReference)), name);

        public Field CreateField(Field field, string name) => this.CreateField(field.fieldRef.FieldType, name);

        public Field CreateField(TypeReference typeReference, string name) =>
            this.IsStatic ? this.DeclaringType.CreateField(Modifiers.PrivateStatic, typeReference, name) : this.DeclaringType.CreateField(Modifiers.Private, typeReference, name);

        public ICode NewCode() => new InstructionsSet(this.type, this);

        internal ILProcessor GetILProcessor() => this.methodDefinition.Body.GetILProcessor();

        #region Variables

        public LocalVariableCollection LocalVariables { get { return new LocalVariableCollection(this.type, this.methodDefinition.Body.Variables); } }

        #endregion Variables

        #region Equitable stuff

        public static implicit operator string(Method value) => value.ToString();

        public static bool operator !=(Method a, Method b) => !(a == b);

        public static bool operator ==(Method a, Method b)
        {
            if (object.Equals(a, null) && object.Equals(b, null))
                return true;

            if (object.Equals(a, null))
                return false;

            return a.Equals(b);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (object.Equals(obj, null))
                return false;

            if (object.ReferenceEquals(obj, this))
                return true;

            if (obj is Method)
                return this.Equals(obj as Method);

            if (obj is MethodDefinition)
                return this.methodDefinition == obj as MethodDefinition;

            if (obj is MethodReference)
                return this.methodReference == obj as MethodReference;

            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Equals(Method other)
        {
            if (object.Equals(other, null))
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.methodDefinition == other.methodDefinition;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => this.methodDefinition.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => this.methodReference.FullName;

        #endregion Equitable stuff
    }
}