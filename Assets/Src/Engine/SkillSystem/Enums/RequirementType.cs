using System;


[Flags]
public enum RequirementType {
    CastStart = 1 << 0,
    CastUpdate = 1 << 1,
    CastComplete = 1 << 2,
    StartAndEnd = (CastStart | CastComplete),
    StartAndUpdate = (CastStart | CastUpdate),
    UpdateAndEnd = (CastUpdate | CastComplete),
    All = (CastStart | CastUpdate | CastComplete),
    Disabled = 1 << 3
}

