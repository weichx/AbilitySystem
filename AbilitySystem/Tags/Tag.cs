using System;

[Serializable]
public struct Tag {
    //todo play with hashing string values somehow to generate something we can bit-test for inclusion
    //could also hash the string and do int compare which should be faster than string compare
    public readonly static Tag Null = new Tag("___NULL___");

    public string name;
    public int hash;

    public Tag(string name) {
        this.name = name;
        hash = name.GetHashCode();
    }

    public static TagCollection operator |(Tag tag1, Tag tag2) {
        return new TagCollection(tag1, tag2);
    }

    public static TagCollection operator |(TagCollection collection, Tag tag) {
        collection.Add(tag);
        return collection;
    }

    public static implicit operator string (Tag tag) {
        return tag.name;
    }
}