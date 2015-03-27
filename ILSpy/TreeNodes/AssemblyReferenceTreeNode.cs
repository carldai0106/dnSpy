﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Diagnostics;
using System.Linq;
using ICSharpCode.Decompiler;
using dnlib.DotNet;

namespace ICSharpCode.ILSpy.TreeNodes
{
	/// <summary>
	/// Node within assembly reference list.
	/// </summary>
	public sealed class AssemblyReferenceTreeNode : ILSpyTreeNode
	{
		readonly AssemblyRef r;
		readonly AssemblyTreeNode parentAssembly;
		
		public AssemblyReferenceTreeNode(AssemblyRef r, AssemblyTreeNode parentAssembly)
		{
			if (parentAssembly == null)
				throw new ArgumentNullException("parentAssembly");
			if (r == null)
				throw new ArgumentNullException("r");
			this.r = r;
			this.parentAssembly = parentAssembly;
			this.LazyLoading = true;
		}

		public AssemblyRef AssemblyNameReference
		{
			get { return r; }
		}
		
		public override object Text {
			get { return ToString(Language); }
		}

		public override string ToString(Language language)
		{
			return CleanUpName(r.Name) + r.MDToken.ToSuffixString();
		}
		
		public override object Icon {
			get { return Images.Assembly; }
		}
		
		public override bool ShowExpander {
			get {
				if (r.IsCorLib())
					EnsureLazyChildren(); // likely doesn't have any children
				return base.ShowExpander;
			}
		}

		AssemblyListTreeNode GetAssemblyListTreeNode() {
			return parentAssembly.Ancestors().OfType<AssemblyListTreeNode>().FirstOrDefault();
		}
		
		public override void ActivateItem(System.Windows.RoutedEventArgs e)
		{
			var assemblyListNode = GetAssemblyListTreeNode();
			Debug.Assert(assemblyListNode != null);
			if (assemblyListNode != null) {
				assemblyListNode.Select(assemblyListNode.FindAssemblyNode(parentAssembly.LoadedAssembly.LookupReferencedAssembly(r)));
				e.Handled = true;
			}
		}
		
		protected override void LoadChildren()
		{
			var assemblyListNode = GetAssemblyListTreeNode();
			Debug.Assert(assemblyListNode != null);
			if (assemblyListNode != null) {
				var refNode = assemblyListNode.FindAssemblyNode(parentAssembly.LoadedAssembly.LookupReferencedAssembly(r));
				if (refNode != null) {
					ModuleDef module = refNode.LoadedAssembly.ModuleDefinition;
					if (module is ModuleDefMD) {
						foreach (var childRef in ((ModuleDefMD)module).GetAssemblyRefs())
							this.Children.Add(new AssemblyReferenceTreeNode(childRef, refNode));
					}
				}
			}
		}
		
		public override void Decompile(Language language, ITextOutput output, DecompilationOptions options)
		{
			if (r.IsContentTypeWindowsRuntime) {
				language.WriteCommentLine(output, r.Name + " [WinRT]");
			} else {
				language.WriteCommentLine(output, r.FullName);
			}
		}

		public override NodePathName NodePathName {
			get { return new NodePathName("asmref", r.FullName); }
		}
	}
}
