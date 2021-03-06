﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnSpy.Contracts.Files.TreeView;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Languages;
using dnSpy.Contracts.Text;
using dnSpy.Contracts.TreeView;
using dnSpy.Properties;

namespace dnSpy.Files.TreeView {
	sealed class BaseTypeFolderNode : FileTreeNodeData, IBaseTypeFolderNode {
		public override Guid Guid => new Guid(FileTVConstants.BASETYPEFOLDER_NODE_GUID);
		public override NodePathName NodePathName => new NodePathName(Guid);
		protected override ImageReference GetIcon(IDotNetImageManager dnImgMgr) => new ImageReference(GetType().Assembly, "BaseTypeClosed");
		protected override ImageReference? GetExpandedIcon(IDotNetImageManager dnImgMgr) => new ImageReference(GetType().Assembly, "BaseTypeOpened");
		public override ITreeNodeGroup TreeNodeGroup { get; }

		readonly TypeDef type;

		public BaseTypeFolderNode(ITreeNodeGroup treeNodeGroup, TypeDef type) {
			this.TreeNodeGroup = treeNodeGroup;
			this.type = type;
		}

		public override void Initialize() => TreeNode.LazyLoading = true;

		public override IEnumerable<ITreeNodeData> CreateChildren() {
			if (type.BaseType != null)
				yield return new BaseTypeNode(Context.FileTreeView.FileTreeNodeGroups.GetGroup(FileTreeNodeGroupType.BaseTypeTreeNodeGroupBaseType), type.BaseType, true);
			foreach (var iface in type.Interfaces)
				yield return new BaseTypeNode(Context.FileTreeView.FileTreeNodeGroups.GetGroup(FileTreeNodeGroupType.InterfaceBaseTypeTreeNodeGroupBaseType), iface.Interface, false);
		}

		protected override void Write(ITextColorWriter output, ILanguage language) =>
			output.Write(BoxedTextColor.Text, dnSpy_Resources.BaseTypeFolder);
		public override FilterType GetFilterType(IFileTreeNodeFilter filter) =>
			filter.GetResult(this).FilterType;

		public void InvalidateChildren() {
			TreeNode.Children.Clear();
			TreeNode.LazyLoading = true;
		}
	}
}
