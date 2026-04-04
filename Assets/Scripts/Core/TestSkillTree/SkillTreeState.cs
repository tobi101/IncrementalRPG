using System;
using System.Collections.Generic;

namespace Core.TestSkillTree
{
    [Serializable]
    public class SkillTreeState
    {
        public List<NodeLevelEntry> nodeLevels = new List<NodeLevelEntry>();

        [NonSerialized]
        private Dictionary<string, int> _dict;

        public void Init()
        {
            _dict = new Dictionary<string, int>();
            foreach (var entry in nodeLevels)
                _dict[entry.nodeId] = entry.level;
        }

        // Returns 0 if the node has never been upgraded (available but not upgraded).
        public int GetLevel(string nodeId)
        {
            if (_dict == null) Init();
            return _dict.TryGetValue(nodeId, out var level) ? level : 0;
        }

        public void SetLevel(string nodeId, int level)
        {
            if (_dict == null) Init();
            _dict[nodeId] = level;
            SyncToList();
        }

        private void SyncToList()
        {
            nodeLevels.Clear();
            foreach (var kvp in _dict)
                nodeLevels.Add(new NodeLevelEntry { nodeId = kvp.Key, level = kvp.Value });
        }
    }

    [Serializable]
    public class NodeLevelEntry
    {
        public string nodeId;
        public int level;
    }
}
