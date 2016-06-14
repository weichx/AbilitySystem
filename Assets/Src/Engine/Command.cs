using System;
using System.Collections.Generic;

public class Command {

    public static Dictionary<string, Command> commandMap = new Dictionary<string, Command>();

    public Command(Entity entity, string arguments) {
        //using != casting
        //using should invoke the context builder if null context passed
    }

}