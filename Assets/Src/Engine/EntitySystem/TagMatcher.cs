using System;

public interface ITaggable {
    TagCollection Tags {get;}
}

public class TagMatcher : IMatcher<ITaggable> {

    public TagCollection tags;

    public bool Match(ITaggable t) {
        return t.Tags.ContainsAny(tags);    
    }

}

