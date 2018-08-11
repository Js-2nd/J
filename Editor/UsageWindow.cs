﻿namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEditor.IMGUI.Controls;
	using UnityEngine;

	public class UsageWindow : EditorWindow
	{
		public static void Show(IEnumerable<SearchNode<string>> search)
		{
			GetWindow<UsageWindow>(nameof(UsageDatabase)).Init(search);
		}

		[SerializeField] TreeViewState state;
		[SerializeField] List<UsageTreeEntry> entries;
		UsageTree tree;

		void Init(IEnumerable<SearchNode<string>> search)
		{
			state = null;
			entries = new List<UsageTreeEntry>(search.Select(node => new UsageTreeEntry
			{
				AssetId = node.Value,
				Parent = node.Parent?.Index ?? -1,
				Depth = node.Depth
			}));
			tree = null;
		}

		void OnGUI()
		{
			if (state == null) state = new TreeViewState();
			if (entries == null) entries = new List<UsageTreeEntry>();
			if (tree == null) tree = new UsageTree(state, entries);
			tree.searchString = EditorGUILayout.TextField(tree.searchString);
			tree.OnGUI(new Rect(0, 20, position.width, position.height));
		}
	}

	[Serializable]
	public struct UsageTreeEntry
	{
		public string AssetId;
		public int Parent;
		public int Depth;
	}

	public class UsageTree : TreeView
	{
		readonly TreeViewItem root;
		readonly UsageTreeItem[] items;

		public UsageTree(TreeViewState state, IReadOnlyList<UsageTreeEntry> entries) : base(state)
		{
			root = new TreeViewItem(-1, -1);
			items = new UsageTreeItem[entries.Count];
			for (var i = 0; i < entries.Count; i++)
			{
				var entry = entries[i];
				var item = items[i] = new UsageTreeItem(i, entry.Depth, entry.AssetId);
				if (entry.Parent < 0) root.AddChild(item);
				else items[entry.Parent].AddChild(item);
			}
			Reload();
			ExpandAll();
		}

		protected override TreeViewItem BuildRoot() => root;

		protected override void SingleClickedItem(int id)
		{
			var item = items.ElementAtOrDefault(id);
			if (item?.Path != null) Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(item.Path);
		}
	}

	sealed class UsageTreeItem : TreeViewItem
	{
		public readonly string Path;
		public readonly string Type;

		public UsageTreeItem(int id, int depth, string assetId) : base(id, depth)
		{
			Path = AssetDatabase.GUIDToAssetPath(assetId);
			if (string.IsNullOrEmpty(Path)) Path = null;
			else Type = AssetDatabase.GetMainAssetTypeAtPath(Path)?.Name;
			displayName = $"[{Type}] {Path ?? assetId}";
		}
	}
}
