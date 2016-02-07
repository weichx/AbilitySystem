using System;
using System.Collections.Generic;

namespace AbilitySystem {
    [Serializable]
    public class TagCollection {

        [UnityEngine.SerializeField]
        private List<Tag> tags = new List<Tag>();

        public TagCollection() {
            tags = new List<Tag>();
        }

        public TagCollection(params string[] inputTags) {
            tags = new List<Tag>();
            if (inputTags != null) {
                for (int i = 0; i < inputTags.Length; i++) {
                    Add(new Tag(inputTags[i]));
                }
            }
        }

        public TagCollection(params Tag[] inputTags) {
            tags = new List<Tag>();
            if (inputTags != null) {
                for (int i = 0; i < inputTags.Length; i++) {
                    Add(inputTags[i]);
                }
            }
        }

        public TagCollection(TagCollection toClone) {
            tags = new List<Tag>(toClone.tags);
        }

        public void Add(Tag tag) {
            if (tags.Contains(tag)) return;
            tags.Add(tag);
        }

        public void Add(TagCollection other) {
            for (int i = 0; i < other.tags.Count; i++) {
                if (!tags.Contains(other.tags[i])) {
                    tags.Add(other.tags[i]);
                }
            }
        }

        public void Remove(Tag tag) {
            tags.Remove(tag);
        }

        public bool Contains(Tag tag) {
            return tags.Contains(tag);
        }

        public TagCollection Union(TagCollection other) {
            var retn = new TagCollection();
            List<Tag> shorter = (tags.Count < other.tags.Count) ? tags : other.tags;
            List<Tag> longer = (tags.Count < other.tags.Count) ? other.tags : tags;
            for (int i = 0; i < shorter.Count; i++) {
                if (longer.Contains(shorter[i])) {
                    retn.Add(shorter[i]);
                }
            }
            return retn;
        }

        public TagCollection Difference(TagCollection other) {
            var retn = new TagCollection();
            List<Tag> shorter = (tags.Count < other.tags.Count) ? tags : other.tags;
            List<Tag> longer = (tags.Count < other.tags.Count) ? other.tags : tags;
            for (int i = 0; i < longer.Count; i++) {
                if (!shorter.Contains(longer[i])) {
                    retn.Add(longer[i]);
                }
            }
            return retn;
        }
    }
}